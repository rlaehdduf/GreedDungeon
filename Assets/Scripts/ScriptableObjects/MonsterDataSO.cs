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
    public class MonsterDataSO : ScriptableObject
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
        public string SpecialSkill;
        public bool IsBoss;
        public string PrefabAddress;
    }
}