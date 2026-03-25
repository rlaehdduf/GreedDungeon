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
event Action<string, UI.Battle.LogType> OnBattleLog;
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
        public event Action<string, UI.Battle.LogType> OnBattleLog;
        public event Action OnPlayerDeath;
        public event Action OnMonsterDeath;

        public BattleManager(IDamageCalculator damageCalculator, ITurnManager turnManager, IGameDataManager gameDataManager)
        {
            _damageCalculator = damageCalculator;
            _turnManager = turnManager;
            _gameDataManager = gameDataManager;
        }

        private void LogBattle(string message, UI.Battle.LogType logType = UI.Battle.LogType.System)
        {
            Debug.Log(message);
            OnBattleLog?.Invoke(message, logType);
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

            LogBattle($"전투: {_monster.Name}");
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

            bool isPlayer = attacker == _player;
            string actionText = skill != null ? skill.Name : "공격";

            int hitCount = skill?.HitCount ?? 1;
            int totalDamage = 0;

            for (int hit = 0; hit < hitCount; hit++)
            {
                if (defender.IsDead) break;

                var result = _damageCalculator.CalculateDamage(attacker, defender, skill);
                defender.TakeDamage(result.Damage);
                totalDamage += result.Damage;
            }

            string dmgText = hitCount > 1 ? $"{totalDamage} ({hitCount}회)" : $"{totalDamage}";
            var logType = isPlayer ? UI.Battle.LogType.Player : UI.Battle.LogType.Monster;
            
            if (isPlayer)
                LogBattle($"{actionText} - {defender.Name} {dmgText} Dmg", logType);
            else
                LogBattle($"{attacker.Name} → {dmgText} Dmg", logType);

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
            var logType = defender == _player ? UI.Battle.LogType.Player : UI.Battle.LogType.Monster;
            LogBattle($"방어", logType);
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
                    LogBattle($"[{data.Name}] HP +{healAmount}", UI.Battle.LogType.Player);
                    break;

                case ConsumableEffectType.Cleanse:
                    target.ClearDebuffs();
                    LogBattle($"[{data.Name}] 디버프 해제", UI.Battle.LogType.Player);
                    break;

                case ConsumableEffectType.Buff:
                    if (data.BuffType != BuffType.None)
                    {
                        target.ApplyBuff(data.BuffType, data.EffectValue, data.Duration);
                        LogBattle($"[{data.Name}] {data.BuffType} +{data.EffectValue}%", UI.Battle.LogType.Player);
                    }
                    break;

                case ConsumableEffectType.Poison:
                case ConsumableEffectType.Burn:
                    var effect = FindStatusEffect(data.EffectType == ConsumableEffectType.Poison ? "Poison" : "Burn");
                    if (effect != null)
                    {
                        target.ApplyStatusEffect(effect, data.Duration);
                        LogBattle($"[{data.Name}] → {target.Name} {effect.Name}", UI.Battle.LogType.Player);
                    }
                    break;

                case ConsumableEffectType.Attack:
                    int damage = (int)data.EffectValue;
                    target.TakeDamage(damage);
                    LogBattle($"[{data.Name}] → {damage} Dmg", UI.Battle.LogType.Player);
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
                LogBattle($"→ {effect.Name} ({duration}턴)");
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
            
            ExecuteMonsterAttack();
            
            if (_player.IsDead)
            {
                CheckBattleEnd();
                return;
            }
            
            EndTurn();
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
            LogBattle($"{_monster.Name} → {damage} Dmg", UI.Battle.LogType.Monster);
            
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
                LogBattle($"→ {effect.Name}", UI.Battle.LogType.Monster);
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
                LogBattle($"승리! +{goldReward}G");
                OnMonsterDeath?.Invoke();
            }
            else
            {
                LogBattle($"패배...");
                OnPlayerDeath?.Invoke();
            }
        }
    }
}