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
        event Action<int, bool> OnPlayerDamaged;
        event Action<int> OnPlayerHealed;
        event Action<SkillType, int> OnAttackEffect;
        event Action<string, UI.Battle.LogType> OnBattleLog;
        event Action OnPlayerDeath;
        event Action OnMonsterDeath;
        event Action OnMonsterTurnStarted;
        event Action<int, int> OnMonsterHealed;
        event Action<BuffType, float, int> OnMonsterBuffApplied;
        event Action<IReadOnlyList<IBattleEntity>, IBattleEntity> OnGaugeUpdated;

        void StartBattle(Player player, Monster monster);
        void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill);
        void ExecuteDefend(IBattleEntity defender);
        bool ExecuteItem(InventoryItem item, IBattleEntity target);
        void EndTurn();
        void ExecuteMonsterAttack();
        void ExecuteMonsterSkill(MonsterSkillDataSO skill);
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
        public event Action<int, bool> OnPlayerDamaged;
        public event Action<int> OnPlayerHealed;
        public event Action<SkillType, int> OnAttackEffect;
        public event Action<string, UI.Battle.LogType> OnBattleLog;
        public event Action OnPlayerDeath;
        public event Action OnMonsterDeath;
        public event Action OnMonsterTurnStarted;
        public event Action<int, int> OnMonsterHealed;
        public event Action<BuffType, float, int> OnMonsterBuffApplied;
        public event Action<IReadOnlyList<IBattleEntity>, IBattleEntity> OnGaugeUpdated;

        public BattleManager(IDamageCalculator damageCalculator, ITurnManager turnManager, IGameDataManager gameDataManager)
        {
            _damageCalculator = damageCalculator;
            _turnManager = turnManager;
            _gameDataManager = gameDataManager;
            
            _turnManager.OnGaugeUpdated += HandleGaugeUpdated;
        }
        
        private void HandleGaugeUpdated(IReadOnlyList<IBattleEntity> entities)
        {
            OnGaugeUpdated?.Invoke(entities, _turnManager.CurrentEntity);
        }

        private void LogBattle(string message, UI.Battle.LogType logType = UI.Battle.LogType.System)
        {
            OnBattleLog?.Invoke(message, logType);
        }

        private bool _battleEnded;

        public void StartBattle(Player player, Monster monster)
        {
            _battleEnded = false;
            CleanupPreviousBattle();
            
            _player = player;
            _monster = monster;
            _monster.InitializeForBattle();

            _battleEntities = new List<IBattleEntity> { _player, _monster };
            _turnManager.Initialize(_battleEntities);

            _monster.OnDamaged += HandleMonsterDamaged;
            
            SubscribeEntityEvents(_player);
            SubscribeEntityEvents(_monster);
            
            OnBattleStarted?.Invoke(_monster);

            LogBattle($"전투: {_monster.Name}");
        }

        private void SubscribeEntityEvents(IBattleEntity entity)
        {
            entity.OnStatusEffectDamage += HandleStatusEffectDamage;
            entity.OnStatusEffectApplied += HandleStatusEffectApplied;
            entity.OnStatusEffectEnded += HandleStatusEffectEnded;
            entity.OnBuffApplied += HandleBuffApplied;
            entity.OnBuffEnded += HandleBuffEnded;
        }

        private void UnsubscribeEntityEvents(IBattleEntity entity)
        {
            entity.OnStatusEffectDamage -= HandleStatusEffectDamage;
            entity.OnStatusEffectApplied -= HandleStatusEffectApplied;
            entity.OnStatusEffectEnded -= HandleStatusEffectEnded;
            entity.OnBuffApplied -= HandleBuffApplied;
            entity.OnBuffEnded -= HandleBuffEnded;
        }

        private void HandleStatusEffectDamage(IBattleEntity entity, ActiveStatusEffect effect, int damage)
        {
            var logType = entity == _player ? UI.Battle.LogType.Player : UI.Battle.LogType.Monster;
            LogBattle($"{effect.Data.Name} → {damage} Dmg", logType);
        }

        private void HandleStatusEffectApplied(IBattleEntity entity, ActiveStatusEffect effect)
        {
            var logType = entity == _player ? UI.Battle.LogType.Player : UI.Battle.LogType.Monster;
            LogBattle($"→ {effect.Data.Name} ({effect.RemainingDuration}턴)", logType);
        }

        private void HandleStatusEffectEnded(IBattleEntity entity, ActiveStatusEffect effect)
        {
            var logType = entity == _player ? UI.Battle.LogType.Player : UI.Battle.LogType.Monster;
            LogBattle($"{effect.Data.Name} 종료", logType);
        }

        private void HandleBuffApplied(IBattleEntity entity, ActiveBuff buff)
        {
            LogBattle($"{buff.Type} +{buff.Value}% ({buff.RemainingDuration}턴)", UI.Battle.LogType.Player);
        }

        private void HandleBuffEnded(IBattleEntity entity, ActiveBuff buff)
        {
            LogBattle($"{buff.Type} 버프 종료", UI.Battle.LogType.Player);
        }

        private void HandleMonsterDamaged(int damage)
        {
            OnMonsterDamaged?.Invoke(_monster, damage);
        }

        private void CleanupPreviousBattle()
        {
            if (_player != null)
            {
                UnsubscribeEntityEvents(_player);
            }
            if (_monster != null)
            {
                _monster.OnDamaged -= HandleMonsterDamaged;
                UnsubscribeEntityEvents(_monster);
            }
        }

        public void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            if (attacker.IsDead || defender.IsDead) return;

            bool isPlayer = attacker == _player;
            bool isPlayerDefender = defender == _player;
            string actionText = skill != null ? skill.Name : "공격";

            SkillType skillType = skill != null ? skill.Type : SkillType.Neutral;
            int hitCount = skill?.HitCount ?? 1;
            
            OnAttackEffect?.Invoke(skillType, hitCount);

            int totalDamage = 0;
            bool isCritical = false;

            for (int hit = 0; hit < hitCount; hit++)
            {
                if (defender.IsDead) break;

                var result = _damageCalculator.CalculateDamage(attacker, defender, skill);
                Debug.Log($"[Damage] Base:{result.BaseDamage} Def:{result.Defense} AfterDef:{result.DamageAfterDefense} Crit:{result.IsCritical}({result.CriticalMultiplier}) Elem:{result.ElementMultiplier} Final:{result.Damage}");
                defender.TakeDamage(result.Damage);
                totalDamage += result.Damage;
                if (result.IsCritical) isCritical = true;
            }

            if (isPlayerDefender)
            {
                OnPlayerDamaged?.Invoke(totalDamage, isCritical);
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
                    if (target == _player)
                        OnPlayerHealed?.Invoke(healAmount);
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
                    var effect = FindStatusEffect(data.StatusEffectID);
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
                    OnMonsterTurnStarted?.Invoke();
                }
            }
}

        public void ExecuteMonsterAttack()
        {
            int damage = _monster.BaseStats.Attack;
            
            if (_player.IsDefending)
            {
                damage = damage / 2;
            }
            
            int defense = _player.TotalStats.Defense;
            damage = Mathf.Max(1, damage - defense / 2);
            
            _player.TakeDamage(damage);
            OnPlayerDamaged?.Invoke(damage, false);
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

        public void ExecuteMonsterSkill(MonsterSkillDataSO skill)
        {
            if (skill == null) return;

            switch (skill.SkillType)
            {
                case MonsterSkillType.Attack:
                    ExecuteMonsterAttackSkill(skill);
                    break;
                case MonsterSkillType.Buff:
                    ExecuteMonsterBuffSkill(skill);
                    break;
                case MonsterSkillType.Debuff:
                    ExecuteMonsterDebuffSkill(skill);
                    break;
                case MonsterSkillType.Heal:
                    ExecuteMonsterHealSkill(skill);
                    break;
            }
        }

        private void ExecuteMonsterAttackSkill(MonsterSkillDataSO skill)
        {
            int baseDamage = _monster.TotalStats.Attack;
            int totalDamage = 0;

            for (int i = 0; i < skill.HitCount; i++)
            {
                int damage = Mathf.RoundToInt(baseDamage * skill.DamageMultiplier);
                if (_player.IsDefending) damage /= 2;
                damage = Mathf.Max(1, damage - _player.TotalStats.Defense / 2);
                _player.TakeDamage(damage);
                totalDamage += damage;
            }

            OnPlayerDamaged?.Invoke(totalDamage, false);
            LogBattle($"{_monster.Name} {skill.Name}! → {totalDamage} Dmg", UI.Battle.LogType.Monster);
        }

        private void ExecuteMonsterBuffSkill(MonsterSkillDataSO skill)
        {
            if (skill.BuffType == BuffType.None) return;

            _monster.ApplyBuff(skill.BuffType, skill.BuffValue, skill.BuffDuration);
            OnMonsterBuffApplied?.Invoke(skill.BuffType, skill.BuffValue, skill.BuffDuration);
            LogBattle($"{_monster.Name} {skill.Name}! {skill.BuffType} +{skill.BuffValue}%", UI.Battle.LogType.Monster);
        }

        private void ExecuteMonsterDebuffSkill(MonsterSkillDataSO skill)
        {
            int damage = Mathf.RoundToInt(_monster.TotalStats.Attack * skill.DamageMultiplier);
            if (_player.IsDefending) damage /= 2;
            damage = Mathf.Max(1, damage - _player.TotalStats.Defense / 2);

            _player.TakeDamage(damage);
            OnPlayerDamaged?.Invoke(damage, false);
            LogBattle($"{_monster.Name} {skill.Name}! → {damage} Dmg", UI.Battle.LogType.Monster);

            if (!string.IsNullOrEmpty(skill.StatusEffectID) &&
                UnityEngine.Random.value * 100 < skill.StatusEffectChance)
            {
                var effect = FindStatusEffect(skill.StatusEffectID);
                if (effect != null)
                {
                    _player.ApplyStatusEffect(effect, effect.Duration);
                    LogBattle($"→ {effect.Name}", UI.Battle.LogType.Monster);
                }
            }
        }

        private void ExecuteMonsterHealSkill(MonsterSkillDataSO skill)
        {
            int healAmount = Mathf.RoundToInt(_monster.BaseStats.MaxHP * skill.HealPercent / 100f);
            _monster.Heal(healAmount);
            OnMonsterHealed?.Invoke(healAmount, _monster.CurrentHP);
            LogBattle($"{_monster.Name} {skill.Name}! HP +{healAmount}", UI.Battle.LogType.Monster);
        }

        private void CheckBattleEnd()
        {
            if (_battleEnded) return;
            if (!IsBattleOver) return;

            _battleEnded = true;

            if (PlayerWon)
            {
                _player.AddKill();
                int goldReward = _monster.GoldDrop;
                _player.AddGold(goldReward);
                
                // 전투 후 20% HP 회복
                int healAmount = (int)(_player.TotalStats.MaxHP * 0.2f);
                if (healAmount > 0)
                {
                    _player.Heal(healAmount);
                }
                
                LogBattle($"승리! +{goldReward}G, HP +{healAmount}");
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