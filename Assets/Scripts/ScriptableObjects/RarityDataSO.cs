using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RarityData", menuName = "GreedDungeon/Data/Rarity")]
    public class RarityDataSO : ScriptableObject
    {
        public int ID;
        public string Name;
        public float StatMultiplier;
        public bool HasSkill;
        public int SkillTierMin;
        public int SkillTierMax;
        public int DropWeight;
    }
}