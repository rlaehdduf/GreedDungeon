using System;
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using GreedDungeon.UI;
using GreedDungeon.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Dungeon.UI
{
    public class ShopUI : UIView
    {
        [Header("Shop Items")]
        [SerializeField] private Transform _shopSlotsContainer;
        [SerializeField] private InventorySlotUI _slotPrefab;
        
        [Header("Player Inventory")]
        [SerializeField] private Transform _playerInventoryContainer;
        [SerializeField] private InventorySlotUI _playerSlotPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private Text _playerGoldText;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private ItemTooltipUI _tooltipUI;
        
        private const int SHOP_SLOT_COUNT = 6;
        private const float RARITY_PRICE_MULTIPLIER = 0.1f;
        
        private static readonly int[] QuantityWeights = { 50, 25, 15, 7, 3 };
        
        private Player _player;
        private IGameDataManager _gameDataManager;
        private readonly List<InventoryItem> _shopItems = new List<InventoryItem>();
        private int _hoveredShopIndex = -1;
        private int _hoveredPlayerIndex = -1;
        
        public event Action OnLeave;
        
        private void Start()
        {
            if (_leaveButton != null)
            {
                _leaveButton.onClick.AddListener(HandleLeave);
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _gameDataManager = Services.Get<IGameDataManager>();
        }
        
        public void ShowShop()
        {
            GenerateShopItems();
            UpdateShopDisplay();
            UpdatePlayerInventoryDisplay();
            UpdatePlayerGold();
            HideTooltip();
            base.Show();
        }
        
        private void GenerateShopItems()
        {
            _shopItems.Clear();
            
            if (_gameDataManager == null) return;
            
            var allEquipment = _gameDataManager.GetAllEquipmentData();
            var allConsumables = _gameDataManager.GetAllConsumableData();
            var usedConsumableIds = new HashSet<int>();
            
            for (int i = 0; i < SHOP_SLOT_COUNT; i++)
            {
                if (UnityEngine.Random.value < 0.6f && allEquipment != null && allEquipment.Count > 0)
                {
                    var equipment = allEquipment[UnityEngine.Random.Range(0, allEquipment.Count)];
                    var rarity = GetWeightedRandomRarity();
                    var skill = RollSkillForEquipment(equipment.SkillPoolType, rarity);
                    var item = new InventoryItem(equipment, rarity, skill);
                    _shopItems.Add(item);
                }
                else if (allConsumables != null && allConsumables.Count > 0)
                {
                    var availableConsumables = new List<ConsumableDataSO>();
                    foreach (var c in allConsumables)
                    {
                        if (!usedConsumableIds.Contains(c.ID))
                        {
                            availableConsumables.Add(c);
                        }
                    }
                    
                    if (availableConsumables.Count == 0)
                    {
                        usedConsumableIds.Clear();
                        availableConsumables = new List<ConsumableDataSO>(allConsumables);
                    }
                    
                    var consumable = availableConsumables[UnityEngine.Random.Range(0, availableConsumables.Count)];
                    usedConsumableIds.Add(consumable.ID);
                    
                    int quantity = GetRandomConsumableQuantity();
                    var item = new InventoryItem(consumable, quantity);
                    _shopItems.Add(item);
                }
            }
        }
        
        private RarityDataSO GetWeightedRandomRarity()
        {
            var rarities = _gameDataManager.GetAllRarityData();
            if (rarities == null || rarities.Count == 0) return null;
            
            int totalWeight = 0;
            var weights = new int[rarities.Count];
            
            for (int i = 0; i < rarities.Count; i++)
            {
                int weight = (rarities.Count - i) * 10;
                weights[i] = weight;
                totalWeight += weight;
            }
            
            int random = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            
            for (int i = 0; i < rarities.Count; i++)
            {
                cumulative += weights[i];
                if (random < cumulative)
                {
                    return rarities[i];
                }
            }
            
            return rarities[0];
        }
        
        private int GetRandomConsumableQuantity()
        {
            int totalWeight = 0;
            foreach (int w in QuantityWeights)
            {
                totalWeight += w;
            }
            
            int random = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            
            for (int i = 0; i < QuantityWeights.Length; i++)
            {
                cumulative += QuantityWeights[i];
                if (random < cumulative)
                {
                    return i + 1;
                }
            }
            
            return 1;
        }
        
        private SkillDataSO RollSkillForEquipment(SkillPoolType poolType, RarityDataSO rarity)
        {
            if (rarity == null || !rarity.HasSkill) return null;
            
            var skillManager = Services.Get<ISkillManager>();
            if (skillManager == null) return null;
            
            return skillManager.GetRandomSkill(poolType, rarity.SkillTierMin, rarity.SkillTierMax);
        }
        
        private void UpdateShopDisplay()
        {
            if (_shopSlotsContainer == null || _slotPrefab == null) return;
            
            foreach (Transform child in _shopSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _shopItems.Count; i++)
            {
                var item = _shopItems[i];
                var slot = Instantiate(_slotPrefab, _shopSlotsContainer);
                slot.SetItem(item);
                slot.SetSlotIndex(i);
                
                int index = i;
                slot.OnLeftClick += (slotIndex) => TryBuyItem(index);
                slot.OnHoverEnter += (hoveredItem, rect) => ShowShopItemTooltip(index, hoveredItem, rect);
                slot.OnHoverExit += HideTooltip;
            }
        }
        
        private void UpdatePlayerInventoryDisplay()
        {
            if (_playerInventoryContainer == null || _playerSlotPrefab == null || _player == null) return;
            
            foreach (Transform child in _playerInventoryContainer)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _player.InventorySize; i++)
            {
                var item = _player.GetItemAt(i);
                if (item == null) continue;
                
                var slot = Instantiate(_playerSlotPrefab, _playerInventoryContainer);
                slot.SetItem(item);
                slot.SetSlotIndex(i);
                
                int index = i;
                slot.OnLeftClick += (slotIndex) => TrySellItem(index);
                slot.OnHoverEnter += (hoveredItem, rect) => ShowPlayerItemTooltip(index, hoveredItem, rect);
                slot.OnHoverExit += HideTooltip;
            }
        }
        
        private void ShowShopItemTooltip(int index, InventoryItem item, RectTransform slotRect)
        {
            if (_tooltipUI != null && item != null)
            {
                int price = GetBuyPrice(item);
                _tooltipUI.Show(item, price, true);
            }
            
            _hoveredShopIndex = index;
            _hoveredPlayerIndex = -1;
        }
        
        private void ShowPlayerItemTooltip(int index, InventoryItem item, RectTransform slotRect)
        {
            if (_tooltipUI != null && item != null)
            {
                int sellPrice = GetSellPrice(item);
                _tooltipUI.Show(item, sellPrice, false);
            }
            
            _hoveredPlayerIndex = index;
            _hoveredShopIndex = -1;
        }
        
        private void HideTooltip()
        {
            if (_tooltipUI != null)
            {
                _tooltipUI.Hide();
            }
            
            _hoveredShopIndex = -1;
            _hoveredPlayerIndex = -1;
        }
        
        private void UpdatePlayerGold()
        {
            if (_playerGoldText != null && _player != null)
            {
                _playerGoldText.text = $"{_player.Gold}G";
            }
        }
        
        private void TryBuyItem(int index)
        {
            if (_player == null || index < 0 || index >= _shopItems.Count) return;
            
            var item = _shopItems[index];
            int price = GetBuyPrice(item);
            
            if (_player.Gold < price)
            {
                Debug.Log("골드가 부족합니다.");
                return;
            }
            
            if (_player.IsInventoryFull())
            {
                Debug.Log("인벤토리가 가득 찼습니다.");
                return;
            }
            
            _player.SpendGold(price);
            _player.TryAddItem(item.Clone());
            _shopItems.RemoveAt(index);
            
            HideTooltip();
            UpdateShopDisplay();
            UpdatePlayerInventoryDisplay();
            UpdatePlayerGold();
        }
        
        private void TrySellItem(int inventoryIndex)
        {
            if (_player == null) return;
            
            var item = _player.GetItemAt(inventoryIndex);
            if (item == null) return;
            
            int sellPrice = GetSellPrice(item);
            
            _player.RemoveItemAt(inventoryIndex);
            _player.AddGold(sellPrice);
            
            HideTooltip();
            UpdatePlayerInventoryDisplay();
            UpdatePlayerGold();
        }
        
        private int GetBuyPrice(InventoryItem item)
        {
            if (item == null) return 0;
            
            if (item.Type == ItemType.Equipment)
            {
                int basePrice = item.Equipment?.BuyPrice ?? 50;
                
                if (item.Rarity != null)
                {
                    var rarities = _gameDataManager?.GetAllRarityData();
                    if (rarities != null)
                    {
                        int rarityIndex = 0;
                        for (int i = 0; i < rarities.Count; i++)
                        {
                            if (rarities[i].ID == item.Rarity.ID)
                            {
                                rarityIndex = i;
                                break;
                            }
                        }
                        float multiplier = 1f + (rarityIndex * RARITY_PRICE_MULTIPLIER / 100f);
                        basePrice = Mathf.RoundToInt(basePrice * multiplier);
                    }
                }
                
                return basePrice;
            }
            else
            {
                int unitPrice = item.Consumable?.BuyPrice ?? 20;
                return unitPrice * item.Quantity;
            }
        }
        
        private int GetSellPrice(InventoryItem item)
        {
            if (item == null) return 0;
            
            if (item.Type == ItemType.Equipment)
            {
                return item.Equipment?.SellPrice ?? 25;
            }
            else
            {
                return (item.Consumable?.SellPrice ?? 10) * item.Quantity;
            }
        }
        
        private void HandleLeave()
        {
            HideTooltip();
            Hide();
            OnLeave?.Invoke();
        }
    }
}