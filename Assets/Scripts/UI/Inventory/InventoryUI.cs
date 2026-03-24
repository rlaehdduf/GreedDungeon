using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.UI;
using UnityEngine;

namespace GreedDungeon.UI.Inventory
{
    public class InventoryUI : UIView
    {
        [Header("Equip Slots")]
        [SerializeField] private EquipSlotUI _weaponSlot;
        [SerializeField] private EquipSlotUI _armorSlot;
        [SerializeField] private EquipSlotUI _accessorySlot;

        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryGrid;
        [SerializeField] private GameObject _slotPrefab;

        [Header("Stats")]
        [SerializeField] private UnityEngine.UI.Text _statsText;

        [Header("Gold")]
        [SerializeField] private UnityEngine.UI.Text _goldText;

        [Header("Popups")]
        [SerializeField] private ConfirmDropPopup _dropPopup;

        private Player _player;
        private readonly List<InventorySlotUI> _inventorySlots = new List<InventorySlotUI>();
        private int _pendingDropIndex = -1;

        public override void Show()
        {
            base.Show();
            RefreshAll();
        }

        public void Setup(Player player)
        {
            _player = player;

            if (_player != null)
            {
                _player.OnInventoryChanged += RefreshAll;
            }

            SetupEquipSlots();
            CreateInventorySlots();
            SetupDropPopup();
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnInventoryChanged -= RefreshAll;
            }
        }

        private void SetupEquipSlots()
        {
            if (_weaponSlot != null)
            {
                _weaponSlot.SetSlotType(EquipmentType.Weapon);
                _weaponSlot.OnUnequipClicked += OnUnequipClicked;
            }

            if (_armorSlot != null)
            {
                _armorSlot.SetSlotType(EquipmentType.Armor);
                _armorSlot.OnUnequipClicked += OnUnequipClicked;
            }

            if (_accessorySlot != null)
            {
                _accessorySlot.SetSlotType(EquipmentType.Accessory);
                _accessorySlot.OnUnequipClicked += OnUnequipClicked;
            }
        }

        private void CreateInventorySlots()
        {
            if (_inventoryGrid == null || _slotPrefab == null) return;

            foreach (Transform child in _inventoryGrid)
            {
                Destroy(child.gameObject);
            }
            _inventorySlots.Clear();

            for (int i = 0; i < PlayerInventoryUI.INVENTORY_SIZE; i++)
            {
                var slotGO = Instantiate(_slotPrefab, _inventoryGrid);
                var slot = slotGO.GetComponent<InventorySlotUI>();

                if (slot != null)
                {
                    slot.SetSlotIndex(i);
                    slot.OnLeftClick += OnSlotLeftClick;
                    slot.OnRightClick += OnSlotRightClick;
                    _inventorySlots.Add(slot);
                }
            }
        }

        private void SetupDropPopup()
        {
            if (_dropPopup != null)
            {
                _dropPopup.OnConfirmed += OnDropConfirmed;
            }
        }

        private void RefreshAll()
        {
            RefreshEquipSlots();
            RefreshInventorySlots();
            RefreshStats();
            RefreshGold();
        }

        private void RefreshStats()
        {
            if (_statsText == null || _player == null) return;

            var stats = _player.TotalStats;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"HP: {stats.MaxHP}");
            sb.AppendLine($"MP: {stats.MaxMP}");
            sb.AppendLine($"ATK: {stats.Attack}");
            sb.AppendLine($"DEF: {stats.Defense}");
            sb.AppendLine($"SPD: {stats.Speed}");
            sb.Append($"CRIT: {stats.CriticalRate:F0}%");

            _statsText.text = sb.ToString();
        }

        private void RefreshEquipSlots()
        {
            if (_player == null) return;

            if (_weaponSlot != null)
                _weaponSlot.SetEquippedItem(_player.GetEquippedItem(EquipmentType.Weapon));

            if (_armorSlot != null)
                _armorSlot.SetEquippedItem(_player.GetEquippedItem(EquipmentType.Armor));

            if (_accessorySlot != null)
                _accessorySlot.SetEquippedItem(_player.GetEquippedItem(EquipmentType.Accessory));
        }

        private void RefreshInventorySlots()
        {
            if (_player == null) return;

            var inventory = _player.Inventory;

            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (i < inventory.Count)
                {
                    _inventorySlots[i].SetItem(inventory[i]);
                }
                else
                {
                    _inventorySlots[i].SetItem(null);
                }
            }
        }

        private void RefreshGold()
        {
            if (_goldText != null && _player != null)
            {
                _goldText.text = $"{_player.Gold}G";
            }
        }

        private void OnSlotLeftClick(int index)
        {
            if (_player == null) return;

            var item = _player.GetItemAt(index);
            if (item == null) return;

            if (item.Type == ItemType.Consumable)
            {
                UseConsumable(index, item);
            }
            else if (item.Type == ItemType.Equipment)
            {
                EquipItem(index);
            }
        }

        private void UseConsumable(int index, InventoryItem item)
        {
            Debug.Log($"[InventoryUI] 아이템 사용: {item.Name}");
            _player.UseItemAt(index);
        }

        private void EquipItem(int index)
        {
            Debug.Log($"[InventoryUI] 장비 장착: {_player.GetItemAt(index)?.Name}");
            _player.EquipItem(index);
        }

        private void OnSlotRightClick(int index)
        {
            if (_player == null) return;

            var item = _player.GetItemAt(index);
            if (item == null) return;

            _pendingDropIndex = index;

            if (_dropPopup != null)
            {
                _dropPopup.Show(item.Name);
            }
        }

        private void OnDropConfirmed()
        {
            if (_pendingDropIndex < 0 || _player == null) return;

            var item = _player.GetItemAt(_pendingDropIndex);
            if (item != null)
            {
                Debug.Log($"[InventoryUI] 아이템 버림: {item.Name}");
                _player.RemoveItemAt(_pendingDropIndex);
            }

            _pendingDropIndex = -1;
        }

        private void OnUnequipClicked(EquipmentType type)
        {
            if (_player == null) return;

            if (_player.Unequip(type))
            {
                Debug.Log($"[InventoryUI] 장비 해제: {type}");
            }
            else
            {
                Debug.Log($"[InventoryUI] 장비 해제 실패 (인벤토리 꽉 참)");
            }
        }
    }

    public static class PlayerInventoryUI
    {
        public const int INVENTORY_SIZE = 21;
    }
}