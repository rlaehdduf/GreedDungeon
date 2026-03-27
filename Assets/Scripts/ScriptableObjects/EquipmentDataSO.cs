using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Accessory
    }

    public enum SkillPoolType
    {
        Neutral,
        Melee,
        Magic,
        Passive,
        Buff,
        Random
    }

    [CreateAssetMenu(fileName = "EquipmentData", menuName = "GreedDungeon/Data/Equipment")]
    public class EquipmentDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        public EquipmentType Type;
        public int HP;
        public int MP;
        public int Attack;
        public int Defense;
        public int Speed;
        [Range(0f, 100f)] public float CriticalRate;
        public SkillPoolType SkillPoolType;
        public int BuyPrice;
        public int SellPrice;
        [TextArea] public string Description;
        public string IconAddress;
        public int FixedRarityID;
        public int FixedSkillID;
        
        int IData.ID => ID;
    }
}