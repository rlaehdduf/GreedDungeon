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

            result.BaseAttack = attacker.TotalStats.Attack;
            result.SkillMultiplier = skill?.EffectValue ?? 1f;
            result.BaseDamage = (int)(result.BaseAttack * result.SkillMultiplier);

            result.Defense = defender.TotalStats.Defense;
            result.DamageAfterDefense = Math.Max(1, result.BaseDamage - result.Defense / 2);

            result.IsCritical = RollCritical(attacker.TotalStats.CriticalRate);
            result.CriticalMultiplier = result.IsCritical ? BASE_CRITICAL_MULTIPLIER : 1f;
            int damageAfterCrit = (int)(result.DamageAfterDefense * result.CriticalMultiplier);

            result.ElementMultiplier = GetElementMultiplierForSkill(skill, defender, out Element attackElement, out Element defenderElement);
            result.AttackElement = attackElement;
            result.DefenderElement = defenderElement;
            result.Damage = (int)(damageAfterCrit * result.ElementMultiplier);

            return result;
        }

        private bool RollCritical(float criticalRate)
        {
            return UnityEngine.Random.Range(0f, 100f) < criticalRate;
        }

        private float GetElementMultiplierForSkill(SkillDataSO skill, IBattleEntity defender, out Element attackElement, out Element defenderElement)
        {
            attackElement = Element.None;
            defenderElement = Element.None;

            if (skill != null && defender is Monster monster)
            {
                attackElement = GetSkillElement(skill);
                defenderElement = monster.Element;
                return GetElementMultiplier(attackElement, defenderElement);
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
        public int BaseAttack { get; set; }
        public float SkillMultiplier { get; set; }
        public int BaseDamage { get; set; }
        public int Defense { get; set; }
        public int DamageAfterDefense { get; set; }
        public float CriticalMultiplier { get; set; }
        public Element AttackElement { get; set; }
        public Element DefenderElement { get; set; }
    }
}