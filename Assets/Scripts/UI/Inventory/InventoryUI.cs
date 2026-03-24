using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GreedDungeon.UI.Inventory
{
    public class InventoryUI : UIView, IPointerClickHandler
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

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUI _tooltip;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, -10f);

        [Header("Popups")]
        [SerializeField] private ConfirmDropPopup _dropPopup;

        [Header("Close")]
        [SerializeField] private GameObject _closeArea;

        private Player _player;
        private readonly List<InventorySlotUI> _inventorySlots = new List<InventorySlotUI>();
        private int _pendingDropIndex = -1;

        public event System.Action<InventoryItem> OnItemUsed;
        public event System.Action OnClosed;

        public override void Show()
        {
            base.Show();
            RefreshAll();
        }

        public override void Hide()
        {
            base.Hide();
            OnClosed?.Invoke();
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
            SetupTooltip();
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnInventoryChanged -= RefreshAll;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Hide();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
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
                    slot.OnHoverEnter += OnSlotHoverEnter;
                    slot.OnHoverExit += OnSlotHoverExit;
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

            if (_closeArea != null)
            {
                var button = _closeArea.GetComponent<UnityEngine.UI.Button>();
                if (button == null)
                {
                    button = _closeArea.AddComponent<UnityEngine.UI.Button>();
                }
                button.onClick.AddListener(Hide);
            }
        }

        private void SetupTooltip()
        {
            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }

        private void OnSlotHoverEnter(InventoryItem item, Vector2 screenPosition)
        {
            if (_tooltip == null || item == null) return;

            _tooltip.Show(item);
            UpdateTooltipPosition(screenPosition);
        }

        private void OnSlotHoverExit()
        {
            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }

        private void UpdateTooltipPosition(Vector2 screenPosition)
        {
            if (_tooltip == null) return;

            var tooltipRect = _tooltip.GetComponent<RectTransform>();
            var parentRect = _tooltip.transform.parent as RectTransform;
            var canvas = GetComponentInParent<Canvas>();

            if (parentRect == null || canvas == null) return;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, screenPosition, canvas.worldCamera, out localPos);

            tooltipRect.anchoredPosition = localPos + _tooltipOffset;
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

            var baseStats = _player.GetBaseStatsWithLevel();
            var bonusStats = _player.GetEquipmentBonusStats();
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine($"LV: {_player.Level}");
            sb.AppendLine(FormatStat("HP", baseStats.MaxHP, bonusStats.MaxHP));
            sb.AppendLine(FormatStat("MP", baseStats.MaxMP, bonusStats.MaxMP));
            sb.AppendLine(FormatStat("ATK", baseStats.Attack, bonusStats.Attack));
            sb.AppendLine(FormatStat("DEF", baseStats.Defense, bonusStats.Defense));
            sb.AppendLine(FormatStat("SPD", baseStats.Speed, bonusStats.Speed));
            sb.Append(FormatStat("CRIT", baseStats.CriticalRate, bonusStats.CriticalRate, "%"));

            _statsText.text = sb.ToString();
        }

        private string FormatStat(string label, int baseValue, int bonusValue, string suffix = "")
        {
            if (bonusValue > 0)
                return $"{label}: {baseValue} (+{bonusValue}){suffix}";
            return $"{label}: {baseValue}{suffix}";
        }

        private string FormatStat(string label, float baseValue, float bonusValue, string suffix = "")
        {
            if (bonusValue > 0)
                return $"{label}: {baseValue:F0}% (+{bonusValue:F0}%){suffix}";
            return $"{label}: {baseValue:F0}%{suffix}";
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
            OnItemUsed?.Invoke(item);
        }

        private void EquipItem(int index)
        {
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
                _player.RemoveItemAt(_pendingDropIndex);
            }

            _pendingDropIndex = -1;
        }

        private void OnUnequipClicked(EquipmentType type)
        {
            if (_player == null) return;
            _player.Unequip(type);
        }
    }

    public static class PlayerInventoryUI
    {
        public const int INVENTORY_SIZE = 21;
    }
}