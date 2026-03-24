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

        private IAssetLoader _assetLoader;
        private InventoryItem _item;
        private int _slotIndex;

        public event Action<int> OnLeftClick;
        public event Action<int> OnRightClick;
        public event Action<InventoryItem, Vector2> OnHoverEnter;
        public event Action OnHoverExit;

        public int SlotIndex => _slotIndex;
        public InventoryItem Item => _item;

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
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
                _backgroundImage.color = new Color(0f, 0f, 0f, 0f);
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
            _backgroundImage.color = new Color(0f, 0f, 0f, 0f);
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
            OnHoverEnter?.Invoke(_item, eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }
    }
}