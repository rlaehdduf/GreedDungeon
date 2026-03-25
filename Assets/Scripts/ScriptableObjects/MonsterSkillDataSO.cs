using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum MonsterSkillType
    {
        Attack,
        Buff,
        Debuff,
        Heal
    }

    [CreateAssetMenu(fileName = "MonsterSkillData", menuName = "GreedDungeon/Data/MonsterSkill")]
    public class MonsterSkillDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        [TextArea] public string Description;
        public MonsterSkillType SkillType;
        public bool IsShared;
        
        [Header("Attack")]
        public float DamageMultiplier = 1f;
        public int HitCount = 1;
        
        [Header("Debuff")]
        public string StatusEffectID;
        [Range(0f, 100f)] public float StatusEffectChance;
        
        [Header("Buff")]
        public BuffType BuffType;
        public float BuffValue;
        public int BuffDuration;
        
        [Header("Heal")]
        public float HealPercent;
        
        int IData.ID => ID;
    }
}