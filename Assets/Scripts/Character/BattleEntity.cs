using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Character
{
    public abstract class BattleEntity : IBattleEntity
    {
        private readonly List<ActiveStatusEffect> _statusEffects = new();
        private readonly List<SkillDataSO> _skills = new();

        public abstract string Name { get; }
        public abstract Stats BaseStats { get; }

        public int CurrentHP { get; private set; }
        public int CurrentMP { get; private set; }
        public bool IsDead => CurrentHP <= 0;

        public IReadOnlyList<ActiveStatusEffect> StatusEffects => _statusEffects;
        public IReadOnlyList<SkillDataSO> Skills => _skills;

        public Stats TotalStats
        {
            get
            {
                var total = BaseStats.Clone();
                total += GetEquipmentStats();
                return total;
            }
        }

        protected BattleEntity()
        {
            CurrentHP = BaseStats.MaxHP;
            CurrentMP = BaseStats.MaxMP;
        }

        protected abstract Stats GetEquipmentStats();

        public void TakeDamage(int damage)
        {
            CurrentHP -= damage;
            if (CurrentHP < 0) CurrentHP = 0;
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

        public void ProcessTurnEnd() { }

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