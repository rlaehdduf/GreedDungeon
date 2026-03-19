using System.Collections.Generic;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Character
{
    public class Player : BattleEntity
    {
        private readonly Dictionary<EquipmentType, EquipmentDataSO> _equippedItems = new();
        private int _gold;

        public override string Name => "Player";
        public override Stats BaseStats { get; }
        public int Gold => _gold;
        public int DungeonLevel { get; private set; }

        public Player() : base()
        {
            BaseStats = new Stats(maxHP: 100, maxMP: 50, attack: 10, defense: 5, speed: 10, criticalRate: 5f);
            _gold = 0;
            DungeonLevel = 1;
        }

        protected override Stats GetEquipmentStats()
        {
            var equipmentStats = new Stats();
            foreach (var equipment in _equippedItems.Values)
            {
                if (equipment != null)
                {
                    equipmentStats.MaxHP += equipment.HP;
                    equipmentStats.MaxMP += equipment.MP;
                    equipmentStats.Attack += equipment.Attack;
                    equipmentStats.Defense += equipment.Defense;
                    equipmentStats.Speed += equipment.Speed;
                    equipmentStats.CriticalRate += equipment.CriticalRate;
                }
            }
            return equipmentStats;
        }

        public void Equip(EquipmentDataSO equipment)
        {
            if (equipment == null) return;
            _equippedItems[equipment.Type] = equipment;
            var skill = GetRandomSkillFromPool(equipment.SkillPoolType);
            if (skill != null) AddSkill(skill);
        }

        public void Unequip(EquipmentType type)
        {
            _equippedItems.Remove(type);
        }

        public EquipmentDataSO GetEquipped(EquipmentType type)
        {
            return _equippedItems.TryGetValue(type, out var equipment) ? equipment : null;
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

        private SkillDataSO GetRandomSkillFromPool(SkillPoolType poolType)
        {
            return null;
        }
    }
}