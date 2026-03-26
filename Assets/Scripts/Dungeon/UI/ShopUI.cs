using System;
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
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
            
            for (int i = 0; i < SHOP_SLOT_COUNT; i++)
            {
                if (UnityEngine.Random.value < 0.6f && allEquipment != null && allEquipment.Count > 0)
                {
                    var equipment = allEquipment[UnityEngine.Random.Range(0, allEquipment.Count)];
                    var item = new InventoryItem(equipment, null, null);
                    _shopItems.Add(item);
                }
                else if (allConsumables != null && allConsumables.Count > 0)
                {
                    var consumable = allConsumables[UnityEngine.Random.Range(0, allConsumables.Count)];
                    var item = new InventoryItem(consumable, 1);
                    _shopItems.Add(item);
                }
            }
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
                return item.Equipment?.BuyPrice ?? 50;
            }
            else
            {
                return item.Consumable?.BuyPrice ?? 20;
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