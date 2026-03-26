using System.Collections.Generic;
using System.Linq;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;

namespace GreedDungeon.Character
{
    public class Player : BattleEntity
    {
        private const int INVENTORY_SIZE = 21;
        
        private readonly List<InventoryItem> _inventory = new List<InventoryItem>(INVENTORY_SIZE);
        private readonly Dictionary<EquipmentType, InventoryItem> _equippedItems = new();
        private int _gold;
        private int _killCount;
        private Stats _originalBaseStats;

        public override string Name => "Player";
        public int Gold => _gold;
        public int Level { get; private set; }
        public int KillCount => _killCount;
        public int DungeonLevel { get; private set; }
        public IReadOnlyList<InventoryItem> Inventory => _inventory;
        public int InventorySize => INVENTORY_SIZE;
        
        public event System.Action OnInventoryChanged;
        public event System.Action OnLevelUp;
        public event System.Action OnSkillsChanged;
        public event System.Action OnStatsChanged;

        public Player()
        {
            _originalBaseStats = new Stats(maxHP: 100, maxMP: 50, attack: 10, defense: 5, speed: 10, criticalRate: 5f);
            InitializeStats(CalculateLevelStats(1));
            _gold = 0;
            Level = 1;
            _killCount = 0;
            DungeonLevel = 1;
            
            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                _inventory.Add(null);
            }
        }

        private Stats CalculateLevelStats(int level)
        {
            var stats = _originalBaseStats.Clone();
            float bonusPerLevel = 0.1f * (level - 1);
            stats.MaxHP += (int)(_originalBaseStats.MaxHP * bonusPerLevel);
            stats.MaxMP += (int)(_originalBaseStats.MaxMP * bonusPerLevel);
            stats.Attack += (int)(_originalBaseStats.Attack * bonusPerLevel);
            stats.Defense += (int)(_originalBaseStats.Defense * bonusPerLevel);
            stats.Speed += (int)(_originalBaseStats.Speed * bonusPerLevel);
            stats.CriticalRate += _originalBaseStats.CriticalRate * bonusPerLevel;
            return stats;
        }

        public int GetExpRequiredForNextLevel()
        {
            return Level;
        }

        public void AddKill()
        {
            _killCount++;
            int required = GetExpRequiredForNextLevel();
            if (_killCount >= required)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            Level++;
            _killCount = 0;
            var newStats = CalculateLevelStats(Level);
            InitializeStats(newStats);
            OnLevelUp?.Invoke();
            OnStatsChanged?.Invoke();
            OnInventoryChanged?.Invoke();
        }

        public bool IsInventoryFull()
        {
            return _inventory.All(item => item != null);
        }

        public int FindEmptySlot()
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] == null) return i;
            }
            return -1;
        }

        public bool TryAddItem(InventoryItem item)
        {
            if (item == null) return false;
            
            if (item.Type == ItemType.Consumable)
            {
                for (int i = 0; i < _inventory.Count; i++)
                {
                    if (_inventory[i] != null && 
                        _inventory[i].Type == ItemType.Consumable && 
                        _inventory[i].Consumable.ID == item.Consumable.ID)
                    {
                        _inventory[i].AddQuantity(item.Quantity);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
            
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0) return false;
            
            _inventory[emptySlot] = item;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool TryAddEquipment(EquipmentDataSO equipment)
        {
            if (equipment == null) return false;
            if (IsInventoryFull()) return false;
            
            var rarity = RollRarity();
            var skill = RollSkillForEquipment(equipment.SkillPoolType, rarity);
            
            Debug.Log($"[Player] 장비 추가: {equipment.Name}, SkillPoolType: {equipment.SkillPoolType}, Rarity: {rarity?.Name}, Skill: {skill?.Name ?? "null"}");
            
            var item = new InventoryItem(equipment, rarity, skill);
            return TryAddItem(item);
        }

        public bool TryAddConsumable(ConsumableDataSO consumable, int count = 1)
        {
            if (consumable == null || count <= 0) return false;
            
            var item = new InventoryItem(consumable, count);
            return TryAddItem(item);
        }

        public InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= _inventory.Count) return null;
            return _inventory[index];
        }

        public void RemoveItemAt(int index)
        {
            if (index < 0 || index >= _inventory.Count) return;
            _inventory[index] = null;
            OnInventoryChanged?.Invoke();
        }

        public void DecrementItemQuantity(InventoryItem item)
        {
            if (item == null) return;
            
            item.RemoveQuantity(1);
            
            if (item.Quantity <= 0)
            {
                int index = _inventory.IndexOf(item);
                if (index >= 0)
                {
                    _inventory[index] = null;
                }
            }
            
            OnInventoryChanged?.Invoke();
        }

        public bool UseItemAt(int index)
        {
            var item = GetItemAt(index);
            if (item == null || item.Type != ItemType.Consumable) return false;
            
            if (item.RemoveQuantity(1))
            {
                if (item.Quantity <= 0)
                {
                    _inventory[index] = null;
                }
                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool EquipItem(int index)
        {
            var item = GetItemAt(index);
            if (item == null || item.Type != ItemType.Equipment) return false;
            
            var currentEquipped = GetEquippedItem(item.Equipment.Type);
            
            if (currentEquipped != null)
            {
                if (currentEquipped.Skill != null)
                {
                    RemoveSkill(currentEquipped.Skill);
                }
                _inventory[index] = currentEquipped;
            }
            else
            {
                _inventory[index] = null;
            }
            
            _equippedItems[item.Equipment.Type] = item;
            if (item.Skill != null)
            {
                AddSkill(item.Skill);
            }
            
            OnStatsChanged?.Invoke();
            OnSkillsChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool Unequip(EquipmentType type)
        {
            var equipped = GetEquippedItem(type);
            if (equipped == null) return false;
            
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0) return false;
            
            if (equipped.Skill != null)
            {
                RemoveSkill(equipped.Skill);
            }
            
            _inventory[emptySlot] = equipped;
            _equippedItems.Remove(type);
            
            OnStatsChanged?.Invoke();
            OnSkillsChanged?.Invoke();
            OnInventoryChanged?.Invoke();
            return true;
        }

        public InventoryItem GetEquippedItem(EquipmentType type)
        {
            return _equippedItems.TryGetValue(type, out var item) ? item : null;
        }

        public SkillDataSO GetEquippedSkill(EquipmentType type)
        {
            var item = GetEquippedItem(type);
            return item?.Skill;
        }

        public int FindItemIndex(int itemId)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] != null && _inventory[i].ID == itemId)
                    return i;
            }
            return -1;
        }

        public InventoryItem FindItem(int itemId)
        {
            foreach (var item in _inventory)
            {
                if (item != null && item.ID == itemId)
                    return item;
            }
            return null;
        }

        public bool UseItemById(int itemId)
        {
            int index = FindItemIndex(itemId);
            if (index < 0) return false;
            return UseItemAt(index);
        }

        protected override Stats GetEquipmentStats()
        {
            var equipmentStats = new Stats();
            
            foreach (var item in _equippedItems.Values)
            {
                if (item?.Equipment != null)
                {
                    var equip = item.Equipment;
                    var multiplier = item.Rarity?.StatMultiplier ?? 1f;
                    
                    equipmentStats.MaxHP += (int)(equip.HP * multiplier);
                    equipmentStats.MaxMP += (int)(equip.MP * multiplier);
                    equipmentStats.Attack += (int)(equip.Attack * multiplier);
                    equipmentStats.Defense += (int)(equip.Defense * multiplier);
                    equipmentStats.Speed += (int)(equip.Speed * multiplier);
                    equipmentStats.CriticalRate += (int)(equip.CriticalRate * multiplier);
                }
            }

            if (Services.IsInitialized)
            {
                var skillManager = Services.Get<ISkillManager>();
                foreach (var skill in Skills)
                {
                    if (skill.Type == SkillType.Passive)
                        skillManager.ApplyPassiveStats(skill, equipmentStats);
                }
            }

            return equipmentStats;
        }

        public Stats GetEquipmentBonusStats()
        {
            return GetEquipmentStats();
        }

        public Stats GetBaseStatsWithLevel()
        {
            return BaseStats.Clone();
        }

        public SkillDataSO GetSkill(int skillId)
        {
            return Skills.FirstOrDefault(s => s.ID == skillId);
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            if (_gold < 0) _gold = 0;
        }

        public bool SpendGold(int amount)
        {
            if (_gold < amount) return false;
            _gold -= amount;
            return true;
        }

        public void AdvanceDungeonLevel()
        {
            DungeonLevel++;
        }

        public void ResetDungeonProgress()
        {
            DungeonLevel = 1;
            _gold = 0;
            ClearAllStatusEffects();
        }

        public void FullRestore()
        {
            SetInitialHP(TotalStats.MaxHP);
            SetInitialMP(TotalStats.MaxMP);
        }

        private RarityDataSO RollRarity()
        {
            if (!Services.IsInitialized) return null;
            
            var gameDataManager = Services.Get<IGameDataManager>();
            var rarities = gameDataManager.GetAllRarityData();
            
            if (rarities == null || rarities.Count == 0) return null;
            
            int totalWeight = 0;
            foreach (var r in rarities)
            {
                totalWeight += r.DropWeight;
            }
            
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            
            foreach (var r in rarities)
            {
                cumulative += r.DropWeight;
                if (roll < cumulative) return r;
            }
            
            return rarities[0];
        }

        private RarityDataSO GetHighestRarity()
        {
            if (!Services.IsInitialized) return null;
            
            var gameDataManager = Services.Get<IGameDataManager>();
            var rarities = gameDataManager.GetAllRarityData();
            
            if (rarities == null || rarities.Count == 0) return null;
            
            RarityDataSO highest = rarities[0];
            foreach (var r in rarities)
            {
                if (r.HasSkill && (highest == null || !highest.HasSkill || r.StatMultiplier > highest.StatMultiplier))
                {
                    highest = r;
                }
            }
            return highest;
        }

        public bool TryAddEquipmentWithHighestRarity(EquipmentDataSO equipment)
        {
            if (equipment == null) return false;
            if (IsInventoryFull()) return false;
            
            var rarity = GetHighestRarity();
            var skill = RollSkillForEquipment(equipment.SkillPoolType, rarity);
            
            Debug.Log($"[Player] 장비 추가(최고레어도): {equipment.Name}, SkillPoolType: {equipment.SkillPoolType}, Rarity: {rarity?.Name}, Skill: {skill?.Name ?? "null"}");
            
            var item = new InventoryItem(equipment, rarity, skill);
            return TryAddItem(item);
        }

        private SkillDataSO RollSkillForEquipment(SkillPoolType poolType, RarityDataSO rarity)
        {
            if (rarity == null || !rarity.HasSkill) return null;
            if (!Services.IsInitialized) return null;
            
            var skillManager = Services.Get<ISkillManager>();
            if (skillManager == null) return null;
            
            return skillManager.GetRandomSkill(poolType, rarity.SkillTierMin, rarity.SkillTierMax);
        }
    }
}