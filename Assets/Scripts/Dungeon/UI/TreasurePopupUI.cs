using System;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.UI;
using GreedDungeon.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Dungeon.UI
{
    public class TreasurePopupUI : UIView
    {
        [Header("UI Elements")]
        [SerializeField] private Text _goldText;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private InventorySlotUI _itemSlotPrefab;
        [SerializeField] private Button _confirmButton;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUI _tooltip;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, -10f);

        private Player _player;
        private IGameDataManager _gameDataManager;
        private InventorySlotUI _currentSlot;

        public event Action OnConfirmed;
        
        private void Start()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(HandleConfirm);
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _gameDataManager = Services.Get<IGameDataManager>();
        }
        
        public void ShowTreasure(int gold, InventoryItem item)
        {
            if (_goldText != null)
            {
                _goldText.text = $"+{gold}G";
            }

            ClearItemSlots();

            if (item != null && _itemContainer != null && _itemSlotPrefab != null)
            {
                var slot = Instantiate(_itemSlotPrefab, _itemContainer);
                slot.SetItem(item);
                slot.OnHoverEnter += OnSlotHoverEnter;
                slot.OnHoverExit += OnSlotHoverExit;
                _currentSlot = slot;
            }

            if (_tooltip != null)
                _tooltip.Hide();

            base.Show();
        }

        private void OnSlotHoverEnter(InventoryItem item, RectTransform slotRect)
        {
            if (_tooltip == null || item == null) return;
            _tooltip.Show(item);
            UpdateTooltipPosition(slotRect);
        }

        private void OnSlotHoverExit()
        {
            if (_tooltip != null)
                _tooltip.Hide();
        }

        private void UpdateTooltipPosition(RectTransform slotRect)
        {
            if (_tooltip == null || slotRect == null) return;

            var tooltipRect = _tooltip.GetComponent<RectTransform>();
            if (tooltipRect == null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Vector3[] slotCorners = new Vector3[4];
            slotRect.GetWorldCorners(slotCorners);

            Vector2 rightPos = (Vector2)(slotCorners[2] + slotCorners[3]) / 2f + _tooltipOffset;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float maxX = canvasCorners[2].x;

            Vector2 targetPos = rightPos;

            tooltipRect.ForceUpdateRectTransforms();
            float tooltipWidth = tooltipRect.rect.width;

            if (targetPos.x + tooltipWidth > maxX)
            {
                targetPos = (Vector2)(slotCorners[0] + slotCorners[1]) / 2f + new Vector2(-tooltipWidth - _tooltipOffset.x, _tooltipOffset.y);
            }

            tooltipRect.position = new Vector3(targetPos.x, targetPos.y, tooltipRect.position.z);
        }
        
        private void ClearItemSlots()
        {
            if (_itemContainer == null) return;

            if (_currentSlot != null)
            {
                _currentSlot.OnHoverEnter -= OnSlotHoverEnter;
                _currentSlot.OnHoverExit -= OnSlotHoverExit;
                _currentSlot = null;
            }

            for (int i = _itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemContainer.GetChild(i).gameObject);
            }
        }
        
        private void HandleConfirm()
        {
            Hide();
            OnConfirmed?.Invoke();
        }
    }
}