using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum ConsumableEffectType
    {
        Heal,
        Cleanse,
        Buff,
        Poison,
        Burn,
        Attack
    }

    public enum ConsumableTarget
    {
        Player,
        Single,
        All
    }

    public enum BuffType
    {
        None,
        Attack,
        Defense,
        Speed
    }

    [CreateAssetMenu(fileName = "ConsumableData", menuName = "GreedDungeon/Data/Consumable")]
    public class ConsumableDataSO : ScriptableObject
    {
        public int ID;
        public string Name;
        public ConsumableEffectType EffectType;
        [Range(0f, 100f)] public float EffectValue;
        public ConsumableTarget Target;
        public string StatusEffectID;
        public int Duration;
        public BuffType BuffType;
        public int BuyPrice;
        public int SellPrice;
        [TextArea] public string Description;
        public string IconAddress;
    }
}