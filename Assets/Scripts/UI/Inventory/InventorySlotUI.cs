using System;
using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GreedDungeon.UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _quantityText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _defaultIcon;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUI _tooltip;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, -10f);

        private IAssetLoader _assetLoader;
        private InventoryItem _item;
        private int _slotIndex;

        public event Action<int> OnLeftClick;
        public event Action<int> OnRightClick;

        public int SlotIndex => _slotIndex;
        public InventoryItem Item => _item;

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_tooltip != null)
                _tooltip.Hide();
        }

        public void SetSlotIndex(int index)
        {
            _slotIndex = index;
        }

        public void SetItem(InventoryItem item)
        {
            _item = item;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_item == null)
            {
                SetEmptySlot();
                return;
            }

            SetItemIcon();
            SetQuantityText();
            SetBackgroundColor();
        }

        private void SetEmptySlot()
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = new Color(1f, 1f, 1f, 0.3f);
            }

            if (_quantityText != null)
                _quantityText.text = "";

            if (_backgroundImage != null)
                _backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        private async void SetItemIcon()
        {
            if (_iconImage == null) return;

            _iconImage.color = Color.white;

            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            string iconAddress = _item.IconAddress;

            if (!string.IsNullOrEmpty(iconAddress) && _assetLoader != null)
            {
                try
                {
                    var sprite = await _assetLoader.LoadAssetAsync<Sprite>(iconAddress);
                    if (sprite != null)
                    {
                        _iconImage.sprite = sprite;
                        return;
                    }
                }
                catch
                {
                }
            }

            if (_defaultIcon != null)
            {
                _iconImage.sprite = _defaultIcon;
            }
            else
            {
                _iconImage.color = new Color(0.5f, 0.5f, 0.8f, 1f);
            }
        }

        private void SetQuantityText()
        {
            if (_quantityText == null) return;

            if (_item.Type == ItemType.Consumable && _item.Quantity > 1)
            {
                _quantityText.text = $"x{_item.Quantity}";
            }
            else
            {
                _quantityText.text = "";
            }
        }

        private void SetBackgroundColor()
        {
            if (_backgroundImage == null) return;

            if (_item.Rarity != null)
            {
                _backgroundImage.color = _item.Rarity.Color * 0.3f;
            }
            else
            {
                _backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_item == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick?.Invoke(_slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke(_slotIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_item == null) return;
            if (_tooltip == null) return;

            _tooltip.Show(_item);
            UpdateTooltipPosition(eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null)
                _tooltip.Hide();
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
    }
}