using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Items
{
    public enum ItemType
    {
        Equipment,
        Consumable
    }

    public class InventoryItem
    {
        public ItemType Type { get; }
        public EquipmentDataSO Equipment { get; }
        public ConsumableDataSO Consumable { get; }
        public RarityDataSO Rarity { get; }
        public SkillDataSO Skill { get; }
        public int Quantity { get; private set; }
        
        public int ID => Type == ItemType.Equipment ? Equipment.ID : Consumable.ID;
        public string Name => Type == ItemType.Equipment ? Equipment.Name : Consumable.Name;
        public string Description => Type == ItemType.Equipment ? Equipment.Description : Consumable.Description;
        public string IconAddress => Type == ItemType.Equipment ? Equipment.IconAddress : Consumable.IconAddress;
        public bool IsEmpty => Type == ItemType.Equipment ? Equipment == null : Consumable == null;

        public InventoryItem(EquipmentDataSO equipment, RarityDataSO rarity, SkillDataSO skill = null)
        {
            Type = ItemType.Equipment;
            Equipment = equipment;
            Consumable = null;
            Rarity = rarity;
            Skill = skill;
            Quantity = 1;
        }

        public InventoryItem(ConsumableDataSO consumable, int quantity = 1)
        {
            Type = ItemType.Consumable;
            Equipment = null;
            Consumable = consumable;
            Rarity = null;
            Quantity = quantity;
        }

        public void AddQuantity(int amount = 1)
        {
            Quantity += amount;
        }

        public bool RemoveQuantity(int amount = 1)
        {
            if (Quantity < amount) return false;
            Quantity -= amount;
            return true;
        }

        public int GetBonusHP()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            int baseValue = Equipment.HP;
            int bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }

        public int GetBonusMP()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            int baseValue = Equipment.MP;
            int bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }

        public int GetBonusAttack()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            int baseValue = Equipment.Attack;
            int bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }

        public int GetBonusDefense()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            int baseValue = Equipment.Defense;
            int bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }

        public int GetBonusSpeed()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            int baseValue = Equipment.Speed;
            int bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }

        public float GetBonusCriticalRate()
        {
            if (Type != ItemType.Equipment || Equipment == null || Rarity == null) return 0;
            float baseValue = Equipment.CriticalRate;
            float bonus = (int)(baseValue * Rarity.StatMultiplier) - baseValue;
            return bonus > 0 ? bonus : 0;
        }
    }
}