using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Character
{
    public interface IBattleEntity
    {
        string Name { get; }
        int CurrentHP { get; }
        int CurrentMP { get; }
        Stats BaseStats { get; }
        Stats TotalStats { get; }
        bool IsDead { get; }
        IReadOnlyList<ActiveStatusEffect> StatusEffects { get; }
        IReadOnlyList<SkillDataSO> Skills { get; }
        IReadOnlyList<ActiveBuff> Buffs { get; }

        void TakeDamage(int damage);
        void Heal(int amount);
        void UseMP(int amount);
        void RestoreMP(int amount);
        void ApplyStatusEffect(StatusEffectDataSO effect, int remainingDuration);
        void RemoveStatusEffect(ActiveStatusEffect effect);
        bool IsDefending { get; }
        void StartDefend();
        void ApplyBuff(BuffType type, float value, int duration);
        void RemoveBuff(ActiveBuff buff);
        void ClearDebuffs();
        void ProcessTurnStart();
        void ProcessTurnEnd();
        void ClearAllStatusEffects();
    }

    public class ActiveStatusEffect
    {
        public StatusEffectDataSO Data { get; }
        public int RemainingDuration { get; set; }

        public ActiveStatusEffect(StatusEffectDataSO data, int duration)
        {
            Data = data;
            RemainingDuration = duration;
        }
    }
}