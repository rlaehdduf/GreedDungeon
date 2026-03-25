using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum Element
    {
        None,
        Fire,
        Water,
        Grass
    }

    [CreateAssetMenu(fileName = "MonsterData", menuName = "GreedDungeon/Data/Monster")]
    public class MonsterDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        public Element Element;
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int Speed;
        [Range(0f, 100f)] public float CriticalRate;
        public int GoldDropMin;
        public int GoldDropMax;
        public string StatusEffectID;
        [Range(0f, 100f)] public float StatusEffectChance;
        public int UniqueSkillID;
        public int SharedSkillID;
        [Range(0f, 100f)] public float SkillChance;
        public bool IsBoss;
        public string PrefabAddress;
        public float ScaleX = 1f;
        public float ScaleY = 1f;
        
        int IData.ID => ID;
    }
}