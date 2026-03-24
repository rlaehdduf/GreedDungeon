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
            UpdateTooltipPosition();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null)
                _tooltip.Hide();
        }

        private void UpdateTooltipPosition()
        {
            if (_tooltip == null) return;

            var tooltipRect = _tooltip.GetComponent<RectTransform>();
            var slotRect = GetComponent<RectTransform>();
            if (tooltipRect == null || slotRect == null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Bounds tooltipBounds = CalculateTooltipBounds(tooltipRect);
            float tooltipWidth = tooltipBounds.size.x;
            float tooltipHeight = tooltipBounds.size.y;

            Vector3[] slotCorners = new Vector3[4];
            slotRect.GetWorldCorners(slotCorners);
            
            Vector2 rightPos = (Vector2)(slotCorners[2] + slotCorners[3]) / 2f + _tooltipOffset;
            Vector2 leftPos = (Vector2)(slotCorners[0] + slotCorners[1]) / 2f + new Vector2(-tooltipWidth - _tooltipOffset.x, _tooltipOffset.y);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            
            float minX = canvasCorners[0].x;
            float maxX = canvasCorners[2].x;
            float minY = canvasCorners[0].y;
            float maxY = canvasCorners[2].y;

            Vector2 targetPos = rightPos;
            
            if (targetPos.x + tooltipWidth > maxX)
            {
                targetPos = leftPos;
            }
            
            if (targetPos.x < minX)
            {
                targetPos.x = minX;
            }
            
            if (targetPos.y - tooltipHeight < minY)
            {
                targetPos.y = minY + tooltipHeight;
            }
            
            if (targetPos.y > maxY)
            {
                targetPos.y = maxY;
            }

            tooltipRect.position = new Vector3(targetPos.x, targetPos.y, tooltipRect.position.z);
        }

        private Bounds CalculateTooltipBounds(RectTransform root)
        {
            var bounds = new Bounds(root.position, Vector3.zero);
            
            var renderers = root.GetComponentsInChildren<UnityEngine.UI.Graphic>();
            bool hasContent = false;
            
            foreach (var renderer in renderers)
            {
                if (!renderer.gameObject.activeInHierarchy) continue;
                
                var rect = renderer.rectTransform;
                if (rect == null) continue;
                
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                
                foreach (var corner in corners)
                {
                    if (!hasContent)
                    {
                        bounds = new Bounds(corner, Vector3.zero);
                        hasContent = true;
                    }
                    else
                    {
                        bounds.Encapsulate(corner);
                    }
                }
            }
            
            return bounds;
        }
    }
}