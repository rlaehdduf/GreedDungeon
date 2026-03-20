using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Items;

namespace GreedDungeon.UI.Inventory
{
    public class InventoryUI : UIView
    {
        [Header("Tabs")]
        [SerializeField] private Button _consumableTab;
        [SerializeField] private Button _equipmentTab;

        [Header("Content")]
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private GameObject _emptyMessage;

        [Header("Detail Panel")]
        [SerializeField] private Text _itemName;
        [SerializeField] private Text _itemDescription;
        [SerializeField] private Text _itemStats;
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _sellButton;

        [Header("Gold")]
        [SerializeField] private Text _goldText;

        private Player _player;
        private readonly List<GameObject> _slotObjects = new();
        private bool _showConsumables = true;

        public event Action<int> OnItemUsed;
        public event Action<int> OnItemEquipped;
        public event Action<int> OnItemSold;

        private ConsumableDataSO _selectedItem;
        private EquipmentDataSO _selectedEquipment;

        private void Start()
        {
            SetupTabs();
            SetupButtons();
        }

        private void SetupTabs()
        {
            if (_consumableTab != null)
                _consumableTab.onClick.AddListener(() => ShowConsumables());

            if (_equipmentTab != null)
                _equipmentTab.onClick.AddListener(() => ShowEquipments());
        }

        private void SetupButtons()
        {
            if (_useButton != null)
                _useButton.onClick.AddListener(UseSelectedItem);

            if (_equipButton != null)
                _equipButton.onClick.AddListener(EquipSelectedItem);

            if (_sellButton != null)
                _sellButton.onClick.AddListener(SellSelectedItem);
        }

        public void Setup(Player player)
        {
            _player = player;
            UpdateGold();
            ShowConsumables();
        }

        public override void Show()
        {
            base.Show();
            RefreshCurrentTab();
        }

        private void UpdateGold()
        {
            if (_goldText != null && _player != null)
                _goldText.text = $"{_player.Gold}G";
        }

        private void ShowConsumables()
        {
            _showConsumables = true;
            ClearSlots();
            
            if (_player == null) return;

            foreach (var kvp in _player.Inventory)
            {
                var item = kvp.Value;
                if (item.Quantity <= 0) continue;

                CreateItemSlot(item);
            }

            UpdateEmptyMessage();
        }

        private void ShowEquipments()
        {
            _showConsumables = false;
            ClearSlots();
            
            if (_player == null) return;

            var slots = new[] 
            { 
                EquipmentType.Weapon, 
                EquipmentType.Armor, 
                EquipmentType.Accessory 
            };

            foreach (var type in slots)
            {
                var equipment = _player.GetEquipped(type);
                CreateEquipmentSlot(type, equipment);
            }

            UpdateEmptyMessage();
        }

        private void RefreshCurrentTab()
        {
            if (_showConsumables) ShowConsumables();
            else ShowEquipments();
        }

        private void CreateItemSlot(ConsumableItem item)
        {
            if (_itemContainer == null || _itemSlotPrefab == null) return;

            var go = Instantiate(_itemSlotPrefab, _itemContainer);
            var slot = go.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.Setup(item);
                slot.OnClicked += () => SelectItem(item.Data);
            }

            _slotObjects.Add(go);
        }

        private void CreateEquipmentSlot(EquipmentType type, EquipmentDataSO equipment)
        {
            if (_itemContainer == null || _itemSlotPrefab == null) return;

            var go = Instantiate(_itemSlotPrefab, _itemContainer);
            var slot = go.GetComponent<EquipmentSlotUI>();
            
            if (slot != null)
            {
                slot.Setup(type, equipment);
                slot.OnClicked += () => SelectEquipment(equipment);
            }

            _slotObjects.Add(go);
        }

        private void ClearSlots()
        {
            foreach (var go in _slotObjects)
            {
                if (go != null) Destroy(go);
            }
            _slotObjects.Clear();
            ClearSelection();
        }

        private void UpdateEmptyMessage()
        {
            if (_emptyMessage != null)
                _emptyMessage.SetActive(_slotObjects.Count == 0);
        }

        private void SelectItem(ConsumableDataSO data)
        {
            _selectedItem = data;
            _selectedEquipment = null;
            ShowItemDetail(data);
        }

        private void SelectEquipment(EquipmentDataSO data)
        {
            _selectedEquipment = data;
            _selectedItem = null;
            ShowEquipmentDetail(data);
        }

        private void ShowItemDetail(ConsumableDataSO data)
        {
            if (_itemName != null) _itemName.text = data.Name;
            if (_itemDescription != null) _itemDescription.text = data.Description;
            
            if (_itemStats != null)
                _itemStats.text = $"효과: {data.EffectType} {data.EffectValue}";

            if (_useButton != null) _useButton.gameObject.SetActive(true);
            if (_equipButton != null) _equipButton.gameObject.SetActive(false);
            if (_sellButton != null) _sellButton.gameObject.SetActive(true);
        }

        private void ShowEquipmentDetail(EquipmentDataSO data)
        {
            if (data == null)
            {
                ClearSelection();
                return;
            }

            if (_itemName != null) _itemName.text = data.Name;
            if (_itemDescription != null) _itemDescription.text = data.Description;
            
            if (_itemStats != null)
            {
                var stats = new List<string>();
                if (data.HP > 0) stats.Add($"HP+{data.HP}");
                if (data.MP > 0) stats.Add($"MP+{data.MP}");
                if (data.Attack > 0) stats.Add($"공격+{data.Attack}");
                if (data.Defense > 0) stats.Add($"방어+{data.Defense}");
                if (data.Speed > 0) stats.Add($"속도+{data.Speed}");
                if (data.CriticalRate > 0) stats.Add($"크리티컬+{data.CriticalRate}%");
                
                _itemStats.text = string.Join("\n", stats);
            }

            if (_useButton != null) _useButton.gameObject.SetActive(false);
            if (_equipButton != null) _equipButton.gameObject.SetActive(true);
            if (_sellButton != null) _sellButton.gameObject.SetActive(true);
        }

        private void ClearSelection()
        {
            _selectedItem = null;
            _selectedEquipment = null;
            
            if (_itemName != null) _itemName.text = "";
            if (_itemDescription != null) _itemDescription.text = "";
            if (_itemStats != null) _itemStats.text = "";
            
            if (_useButton != null) _useButton.gameObject.SetActive(false);
            if (_equipButton != null) _equipButton.gameObject.SetActive(false);
            if (_sellButton != null) _sellButton.gameObject.SetActive(false);
        }

        private void UseSelectedItem()
        {
            if (_selectedItem != null)
            {
                OnItemUsed?.Invoke(_selectedItem.ID);
                RefreshCurrentTab();
            }
        }

        private void EquipSelectedItem()
        {
            if (_selectedEquipment != null)
            {
                OnItemEquipped?.Invoke(_selectedEquipment.ID);
                RefreshCurrentTab();
            }
        }

        private void SellSelectedItem()
        {
            if (_selectedItem != null)
            {
                OnItemSold?.Invoke(_selectedItem.ID);
                RefreshCurrentTab();
            }
            else if (_selectedEquipment != null)
            {
                OnItemSold?.Invoke(_selectedEquipment.ID);
                RefreshCurrentTab();
            }
        }
    }
}