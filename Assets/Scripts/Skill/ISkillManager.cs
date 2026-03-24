using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Skill
{
    public interface ISkillManager
    {
        SkillDataSO GetRandomSkill(SkillPoolType poolType);
        SkillDataSO GetRandomSkill(SkillPoolType poolType, int minTier, int maxTier);

        bool IsOnCooldown(int skillId);
        int GetRemainingCooldown(int skillId);
        void StartCooldown(int skillId, int turns);
        void ReduceAllCooldowns();
        void ResetCooldowns();

        void ExecuteSkill(SkillDataSO skill, IBattleEntity caster, IBattleEntity target);

        void ApplyPassiveStats(SkillDataSO skill, Stats stats);
    }
}