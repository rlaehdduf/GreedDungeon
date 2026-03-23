using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum SkillType
    {
        Common,
        Melee,
        Magic,
        Passive,
        Buff
    }

    public enum EffectType
    {
        Damage,
        Passive,
        Buff
    }

    public enum TargetType
    {
        Single,
        All,
        Player
    }

    [CreateAssetMenu(fileName = "SkillData", menuName = "GreedDungeon/Data/Skill")]
    public class SkillDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        public SkillType Type;
        public int MPCost;
        [TextArea] public string Description;
        public EffectType EffectType;
        public float EffectValue;
        public string ValueFloat;
        public int HitCount;
        public int Duration;
        public TargetType Target;
        public string StatusEffectID;
        [Range(0f, 100f)] public float StatusEffectChance;
        public int Cooldown;
        public int Tier;
        public string IconAddress;
        public Element Element;
        
        int IData.ID => ID;
    }
}