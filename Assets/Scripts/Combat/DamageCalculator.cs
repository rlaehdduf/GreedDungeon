using System;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Combat
{
    public interface IDamageCalculator
    {
        DamageResult CalculateDamage(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill);
        float GetElementMultiplier(Element attacker, Element defender);
    }

    public class DamageCalculator : IDamageCalculator
    {
        private const float BASE_CRITICAL_MULTIPLIER = 1.5f;

        public DamageResult CalculateDamage(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            var result = new DamageResult();

            int baseDamage = CalculateBaseDamage(attacker, skill);
            int defense = defender.TotalStats.Defense;
            int finalDamage = Math.Max(1, baseDamage - defense / 2);

            result.IsCritical = RollCritical(attacker.TotalStats.CriticalRate);
            if (result.IsCritical)
                finalDamage = (int)(finalDamage * BASE_CRITICAL_MULTIPLIER);

            float elementMultiplier = GetElementMultiplierForSkill(skill, defender);
            finalDamage = (int)(finalDamage * elementMultiplier);

            result.Damage = finalDamage;
            result.ElementMultiplier = elementMultiplier;

            return result;
        }

        private int CalculateBaseDamage(IBattleEntity attacker, SkillDataSO skill)
        {
            int baseAttack = attacker.TotalStats.Attack;
            float skillMultiplier = skill?.EffectValue ?? 1f;
            return (int)(baseAttack * skillMultiplier);
        }

        private bool RollCritical(float criticalRate)
        {
            return UnityEngine.Random.Range(0f, 100f) < criticalRate;
        }

        private float GetElementMultiplierForSkill(SkillDataSO skill, IBattleEntity defender)
        {
            if (skill == null) return 1f;
            if (defender is Monster monster)
            {
                Element skillElement = GetSkillElement(skill);
                return GetElementMultiplier(skillElement, monster.Element);
            }
            return 1f;
        }

        private Element GetSkillElement(SkillDataSO skill)
        {
            return skill.Name switch
            {
                string s when s.Contains("파이어") || s.Contains("화염") => Element.Fire,
                string s when s.Contains("워터") || s.Contains("물") => Element.Water,
                string s when s.Contains("그라스") || s.Contains("풀") => Element.Grass,
                _ => Element.None
            };
        }

        public float GetElementMultiplier(Element attacker, Element defender)
        {
            if (attacker == Element.None || defender == Element.None)
                return 1f;

            return (attacker, defender) switch
            {
                (Element.Fire, Element.Grass) => 1.5f,
                (Element.Water, Element.Fire) => 1.5f,
                (Element.Grass, Element.Water) => 1.5f,
                (Element.Fire, Element.Water) => 0.5f,
                (Element.Water, Element.Grass) => 0.5f,
                (Element.Grass, Element.Fire) => 0.5f,
                _ => 1f
            };
        }
    }

    public class DamageResult
    {
        public int Damage { get; set; }
        public bool IsCritical { get; set; }
        public float ElementMultiplier { get; set; }
    }
}