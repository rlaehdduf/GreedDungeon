using System;
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;

namespace GreedDungeon.Combat
{
    public interface IBattleManager
    {
        event Action<Monster> OnBattleStarted;
        event Action<Monster, int> OnMonsterDamaged;
        event Action<string> OnBattleLog;
        event Action OnPlayerDeath;
        event Action OnMonsterDeath;
        
        void StartBattle(Player player, Monster monster);
        void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill);
        void ExecuteDefend(IBattleEntity defender);
        bool ExecuteItem(InventoryItem item, IBattleEntity target);
        void EndTurn();
        void ExecuteMonsterTurn();
        bool IsBattleOver { get; }
        bool PlayerWon { get; }
    }

    public class BattleManager : IBattleManager
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly ITurnManager _turnManager;
        private readonly IGameDataManager _gameDataManager;

        private Player _player;
        private Monster _monster;
        private Monster _previousMonster;
        private List<IBattleEntity> _battleEntities;

        public bool IsBattleOver => _player.IsDead || _monster.IsDead;
        public bool PlayerWon => _monster.IsDead && !_player.IsDead;

        public event Action<Monster> OnBattleStarted;
        public event Action<Monster, int> OnMonsterDamaged;
        public event Action<string> OnBattleLog;
        public event Action OnPlayerDeath;
        public event Action OnMonsterDeath;

        public BattleManager(IDamageCalculator damageCalculator, ITurnManager turnManager, IGameDataManager gameDataManager)
        {
            _damageCalculator = damageCalculator;
            _turnManager = turnManager;
            _gameDataManager = gameDataManager;
        }

        private void LogBattle(string message)
        {
            Debug.Log(message);
            OnBattleLog?.Invoke(message);
        }

        public void StartBattle(Player player, Monster monster)
        {
            CleanupPreviousBattle();
            
            _player = player;
            _monster = monster;
            _monster.InitializeForBattle();

            _battleEntities = new List<IBattleEntity> { _player, _monster };
            _turnManager.Initialize(_battleEntities);

            _monster.OnDamaged += HandleMonsterDamaged;
            OnBattleStarted?.Invoke(_monster);

            LogBattle($"═══ 전투 시작: {_monster.Name} ═══");
        }

        private void HandleMonsterDamaged(int damage)
        {
            OnMonsterDamaged?.Invoke(_monster, damage);
        }

        private void CleanupPreviousBattle()
        {
            if (_monster != null)
            {
                _monster.OnDamaged -= HandleMonsterDamaged;
            }
        }

        public void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            if (attacker.IsDead || defender.IsDead) return;

            string attackerName = attacker == _player ? "플레이어" : attacker.Name;
            string actionText = skill != null ? $"스킬 [{skill.Name}]" : "공격";

            if (skill != null && skill.MPCost > 0)
            {
                if (attacker.CurrentMP < skill.MPCost)
                {
                    LogBattle($"MP 부족! (필요: {skill.MPCost})");
                    return;
                }
                attacker.UseMP(skill.MPCost);
            }

            int hitCount = skill?.HitCount ?? 1;
            int totalDamage = 0;

            for (int hit = 0; hit < hitCount; hit++)
            {
                if (defender.IsDead) break;

                var result = _damageCalculator.CalculateDamage(attacker, defender, skill);
                defender.TakeDamage(result.Damage);
                totalDamage += result.Damage;
            }

            string resultText = hitCount > 1 ? $"{hitCount}연타 {totalDamage}" : $"{totalDamage}";
            LogBattle($"{attackerName} {actionText} → {defender.Name} {resultText} 데미지");

            ApplyStatusEffectFromSkill(skill, defender);

            if (defender.IsDead)
            {
                CheckBattleEnd();
            }
        }

        public void ExecuteDefend(IBattleEntity defender)
        {
            if (defender.IsDead) return;

            defender.StartDefend();
            LogBattle($"방어 태세");
        }

        public bool ExecuteItem(InventoryItem item, IBattleEntity target)
        {
            if (item == null || item.Type != ItemType.Consumable) return false;
            if (item.Quantity <= 0) return false;
            if (target == null || target.IsDead) return false;

            var data = item.Consumable;

            switch (data.EffectType)
            {
                case ConsumableEffectType.Heal:
                    int healAmount = (int)data.EffectValue;
                    target.Heal(healAmount);
                    LogBattle($"아이템 [{data.Name}] → HP +{healAmount}");
                    break;

                case ConsumableEffectType.Cleanse:
                    target.ClearDebuffs();
                    LogBattle($"아이템 [{data.Name}] → 디버프 해제");
                    break;

                case ConsumableEffectType.Buff:
                    if (data.BuffType != BuffType.None)
                    {
                        target.ApplyBuff(data.BuffType, data.EffectValue, data.Duration);
                        LogBattle($"아이템 [{data.Name}] → {data.BuffType} +{data.EffectValue}% ({data.Duration}턴)");
                    }
                    break;

                case ConsumableEffectType.Poison:
                case ConsumableEffectType.Burn:
                    var effect = FindStatusEffect(data.EffectType == ConsumableEffectType.Poison ? "Poison" : "Burn");
                    if (effect != null)
                    {
                        target.ApplyStatusEffect(effect, data.Duration);
                        LogBattle($"아이템 [{data.Name}] → {target.Name} {effect.Name}");
                    }
                    break;

                case ConsumableEffectType.Attack:
                    int damage = (int)data.EffectValue;
                    target.TakeDamage(damage);
                    LogBattle($"아이템 [{data.Name}] → {target.Name} {damage} 데미지");
                    if (target.IsDead)
                    {
                        CheckBattleEnd();
                    }
                    break;
            }

            _player.DecrementItemQuantity(item);
            
            return true;
        }

        private void ApplyStatusEffectFromSkill(SkillDataSO skill, IBattleEntity target)
        {
            if (skill == null || string.IsNullOrEmpty(skill.StatusEffectID)) return;

            float roll = UnityEngine.Random.value * 100;
            if (roll > skill.StatusEffectChance) return;

            var effect = FindStatusEffect(skill.StatusEffectID);
            if (effect != null)
            {
                int duration = skill.Duration > 0 ? skill.Duration : effect.Duration;
                target.ApplyStatusEffect(effect, duration);
                LogBattle($"→ {target.Name} {effect.Name} ({duration}턴)");
            }
        }

        private StatusEffectDataSO FindStatusEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId) || _gameDataManager == null) return null;
            if (int.TryParse(effectId, out int id))
            {
                return _gameDataManager.GetStatusEffectData(id);
            }
            return null;
        }

        public void EndTurn()
        {
            var current = _turnManager.CurrentEntity;
            current?.ProcessTurnEnd();

            if (Services.IsInitialized)
            {
                var skillManager = Services.Get<ISkillManager>();
                skillManager?.ReduceAllCooldowns();
            }

            _turnManager.NextTurn();

            var nextEntity = _turnManager.CurrentEntity;
            if (nextEntity != null)
            {
                nextEntity.ProcessTurnStart();
                if (nextEntity.IsDead)
                {
                    CheckBattleEnd();
                    return;
                }
                
                if (nextEntity == _monster)
                {
                    ExecuteMonsterTurn();
                }
            }
        }

        public void ExecuteMonsterTurn()
        {
            if (_monster == null || _monster.IsDead || _player == null || _player.IsDead) return;

            LogBattle($"[{_monster.Name}의 턴]");
            
            ExecuteMonsterAttack();
            
            if (_player.IsDead)
            {
                CheckBattleEnd();
            }
        }

        private void ExecuteMonsterAttack()
        {
            int damage = _monster.BaseStats.Attack;
            
            if (_player.IsDefending)
            {
                damage = damage / 2;
            }
            
            int defense = _player.TotalStats.Defense;
            damage = Mathf.Max(1, damage - defense / 2);
            
            _player.TakeDamage(damage);
            LogBattle($"{_monster.Name} 공격 → 플레이어 {damage} 데미지");
            
            TryApplyMonsterStatusEffect();
        }

        private void TryApplyMonsterStatusEffect()
        {
            if (!_monster.HasStatusEffectAttack) return;
            
            var effect = _monster.GetStatusEffectForAttack();
            if (effect == null) return;
            
            float roll = UnityEngine.Random.value * 100;
            if (roll < _monster.GetStatusEffectChance())
            {
                _player.ApplyStatusEffect(effect, effect.Duration);
                LogBattle($"→ 플레이어 {effect.Name}");
            }
        }

        private void CheckBattleEnd()
        {
            if (!IsBattleOver) return;

            if (PlayerWon)
            {
                _player.AddKill();
                int goldReward = _monster.GoldDrop;
                _player.AddGold(goldReward);
                LogBattle($"═══ 승리! +{goldReward}G ═══");
                OnMonsterDeath?.Invoke();
            }
            else
            {
                LogBattle($"═══ 패배... ═══");
                OnPlayerDeath?.Invoke();
            }
        }
    }
}