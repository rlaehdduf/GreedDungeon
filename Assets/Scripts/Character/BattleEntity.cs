using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;

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
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];
                ProcessStatusEffectDamage(effect);
                effect.RemainingDuration--;
                if (effect.RemainingDuration <= 0)
                    _statusEffects.RemoveAt(i);
            }
        }

        private void ProcessStatusEffectDamage(ActiveStatusEffect effect)
        {
            int damage = effect.Data.DamagePerTurn;
            damage += (int)(CurrentHP * effect.Data.DamageCurrentPercent);
            damage += (int)(TotalStats.MaxHP * effect.Data.DamageMaxPercent);
            if (damage > 0) TakeDamage(damage);
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