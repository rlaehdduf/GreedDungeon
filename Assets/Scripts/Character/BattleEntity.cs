using System;
using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Character
{
    public abstract class BattleEntity : IBattleEntity
    {
        private readonly List<ActiveStatusEffect> _statusEffects = new();
        private readonly List<SkillDataSO> _skills = new();
        private readonly List<ActiveBuff> _buffs = new();
        private Stats _baseStats;
        private Stats _cachedTotalStats;
        private bool _statsCacheDirty = true;

        public abstract string Name { get; }
        public Stats BaseStats => _baseStats;

        public int CurrentHP { get; private set; }
        public int CurrentMP { get; private set; }
        public bool IsDead => CurrentHP <= 0;
        public bool IsDefending { get; private set; }

        public IReadOnlyList<ActiveStatusEffect> StatusEffects => _statusEffects;
        public IReadOnlyList<SkillDataSO> Skills => _skills;
        public IReadOnlyList<ActiveBuff> Buffs => _buffs;

        public event Action<int> OnDamaged;

        public Stats TotalStats
        {
            get
            {
                if (_statsCacheDirty)
                {
                    _cachedTotalStats = CalculateTotalStats();
                    _statsCacheDirty = false;
                }
                return _cachedTotalStats;
            }
        }

        private Stats CalculateTotalStats()
        {
            var total = BaseStats.Clone();
            total += GetEquipmentStats();
            ApplyBuffModifiers(total);
            return total;
        }

        private void InvalidateStatsCache()
        {
            _statsCacheDirty = true;
        }

        private void ApplyBuffModifiers(Stats stats)
        {
            foreach (var buff in _buffs)
            {
                switch (buff.Type)
                {
                    case BuffType.Attack:
                        stats.Attack = (int)(stats.Attack * (1 + buff.Value / 100f));
                        break;
                    case BuffType.Defense:
                        stats.Defense = (int)(stats.Defense * (1 + buff.Value / 100f));
                        break;
                    case BuffType.Speed:
                        stats.Speed = (int)(stats.Speed * (1 + buff.Value / 100f));
                        break;
                }
            }
        }

        protected BattleEntity()
        {
        }

        protected void InitializeStats(Stats baseStats)
        {
            _baseStats = baseStats;
            CurrentHP = _baseStats.MaxHP;
            CurrentMP = _baseStats.MaxMP;
        }

        protected abstract Stats GetEquipmentStats();

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            if (CurrentHP < 0) CurrentHP = 0;
            OnDamaged?.Invoke(damage);
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > TotalStats.MaxHP) CurrentHP = TotalStats.MaxHP;
        }

        public void UseMP(int amount)
        {
            CurrentMP -= amount;
            if (CurrentMP < 0) CurrentMP = 0;
        }

        public void RestoreMP(int amount)
        {
            CurrentMP += amount;
            if (CurrentMP > TotalStats.MaxMP) CurrentMP = TotalStats.MaxMP;
        }

        public void ApplyStatusEffect(StatusEffectDataSO effect, int remainingDuration)
        {
            var existing = _statusEffects.Find(e => e.Data.ID == effect.ID);
            if (existing != null)
            {
                existing.RemainingDuration = remainingDuration;
                return;
            }
            _statusEffects.Add(new ActiveStatusEffect(effect, remainingDuration));
        }

        public void RemoveStatusEffect(ActiveStatusEffect effect)
        {
            _statusEffects.Remove(effect);
        }

        public void ProcessTurnStart()
        {
            if (_statusEffects.Count > 0)
            {
                Debug.Log($"  [{Name}] 상태이상 처리 시작");
            }

            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];
                ProcessStatusEffectDamage(effect);
                effect.RemainingDuration--;
                
                if (effect.RemainingDuration <= 0)
                {
                    Debug.Log($"    [{effect.Data.Name}] 효과 종료");
                    _statusEffects.RemoveAt(i);
                }
                else
                {
                    Debug.Log($"    [{effect.Data.Name}] 남은 지속: {effect.RemainingDuration}턴");
                }
            }
        }

        private void ProcessStatusEffectDamage(ActiveStatusEffect effect)
        {
            if (effect.Data.SkipTurn)
            {
                Debug.Log($"    [{effect.Data.Name}] 턴 스킵!");
            }

            int baseDamage = effect.Data.DamagePerTurn;
            int currentHpDamage = (int)(CurrentHP * effect.Data.DamageCurrentPercent);
            int maxHpDamage = (int)(TotalStats.MaxHP * effect.Data.DamageMaxPercent);
            int totalDamage = baseDamage + currentHpDamage + maxHpDamage;

            if (totalDamage > 0)
            {
                string damageBreakdown = "";
                if (baseDamage > 0) damageBreakdown += $"고정:{baseDamage}";
                if (currentHpDamage > 0) damageBreakdown += $" 현재HP%:{currentHpDamage}";
                if (maxHpDamage > 0) damageBreakdown += $" 최대HP%:{maxHpDamage}";

                TakeDamage(totalDamage);
                Debug.Log($"    [{effect.Data.Name}] 데미지: {totalDamage} ({damageBreakdown.Trim()}) → HP: {CurrentHP}/{TotalStats.MaxHP}");
            }
        }

        public void ProcessTurnEnd() 
        { 
            IsDefending = false;
            ProcessBuffDurations();
        }

        private void ProcessBuffDurations()
        {
            if (_buffs.Count > 0)
            {
                Debug.Log($"  [{Name}] 버프 지속시간 처리");
            }

            bool anyBuffRemoved = false;
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.RemainingDuration--;
                
                if (buff.RemainingDuration <= 0)
                {
                    Debug.Log($"    [{buff.Type}] 버프 종료");
                    _buffs.RemoveAt(i);
                    anyBuffRemoved = true;
                }
                else
                {
                    Debug.Log($"    [{buff.Type}] 남은 지속: {buff.RemainingDuration}턴");
                }
            }
            
            if (anyBuffRemoved)
            {
                InvalidateStatsCache();
            }
        }

        public void StartDefend()
        {
            IsDefending = true;
            Debug.Log($"  [{Name}] 방어 태세! 받는 데미지 50% 감소");
        }

        public void ApplyBuff(BuffType type, float value, int duration)
        {
            var existing = _buffs.Find(b => b.Type == type);
            if (existing != null)
            {
                existing.RemainingDuration = duration;
                Debug.Log($"  [{Name}] {type} 버프 갱신: {value}% (지속: {duration}턴)");
                return;
            }
            _buffs.Add(new ActiveBuff(type, value, duration));
            InvalidateStatsCache();
            Debug.Log($"  [{Name}] {type} 버프 적용: +{value}% (지속: {duration}턴)");
        }

        public void RemoveBuff(ActiveBuff buff)
        {
            _buffs.Remove(buff);
            InvalidateStatsCache();
        }

        public void ClearDebuffs()
        {
            int count = _statusEffects.Count;
            _statusEffects.Clear();
            Debug.Log($"  [{Name}] 디버프 {count}개 해제");
        }

        public void ClearAllStatusEffects()
        {
            _statusEffects.Clear();
        }

        protected void AddSkill(SkillDataSO skill)
        {
            if (skill != null && !_skills.Contains(skill))
                _skills.Add(skill);
        }

        protected void SetInitialHP(int hp)
        {
            CurrentHP = hp;
        }

        protected void SetInitialMP(int mp)
        {
            CurrentMP = mp;
        }
    }
}