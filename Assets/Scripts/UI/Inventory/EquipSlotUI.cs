using System;
using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GreedDungeon.UI.Inventory
{
    public class EquipSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _equippedLabel;
        [SerializeField] private Sprite _defaultIcon;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUI _tooltip;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, -10f);

        private IAssetLoader _assetLoader;
        private EquipmentType _slotType;
        private InventoryItem _equippedItem;

        public event Action<EquipmentType> OnUnequipClicked;

        public EquipmentType SlotType => _slotType;

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_tooltip != null)
                _tooltip.Hide();

            if (_equippedLabel != null)
                _equippedLabel.SetActive(false);
        }

        public void SetSlotType(EquipmentType type)
        {
            _slotType = type;
        }

        public void SetEquippedItem(InventoryItem item)
        {
            _equippedItem = item;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_equippedItem == null || _equippedItem.Equipment == null)
            {
                SetEmptySlot();
                return;
            }

            SetItemDisplay();
        }

        private void SetEmptySlot()
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.color = new Color(1f, 1f, 1f, 0.3f);
            }

            if (_backgroundImage != null)
                _backgroundImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            if (_equippedLabel != null)
                _equippedLabel.SetActive(false);
        }

        private async void SetItemDisplay()
        {
            var equipment = _equippedItem.Equipment;

            if (_backgroundImage != null)
            {
                if (_equippedItem.Rarity != null)
                {
                    _backgroundImage.color = _equippedItem.Rarity.Color * 0.3f;
                }
                else
                {
                    _backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
            }

            if (_equippedLabel != null)
                _equippedLabel.SetActive(true);

            if (_iconImage != null)
            {
                _iconImage.color = Color.white;

                if (_assetLoader == null && Services.IsInitialized)
                {
                    _assetLoader = Services.Get<IAssetLoader>();
                }

                if (!string.IsNullOrEmpty(equipment.IconAddress) && _assetLoader != null)
                {
                    try
                    {
                        var sprite = await _assetLoader.LoadAssetAsync<Sprite>(equipment.IconAddress);
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
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_equippedItem == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnUnequipClicked?.Invoke(_slotType);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_equippedItem == null) return;
            if (_tooltip == null) return;

            _tooltip.Show(_equippedItem);
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