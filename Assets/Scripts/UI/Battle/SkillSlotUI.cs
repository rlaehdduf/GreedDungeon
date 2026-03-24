using System;
using System.Threading.Tasks;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace GreedDungeon.UI.Battle
{
    public class SkillSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slots")]
        [SerializeField] private Button _slot1;
        [SerializeField] private Button _slot2;
        [SerializeField] private Button _slot3;

        [Header("Icon Images")]
        [SerializeField] private Image _icon1;
        [SerializeField] private Image _icon2;
        [SerializeField] private Image _icon3;

        [Header("Tooltip")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private Text _tooltipName;
        [SerializeField] private Text _tooltipDesc;
        [SerializeField] private Text _tooltipCooldown;
        [SerializeField] private Vector2 _tooltipOffset = new Vector2(10f, 10f);

        [Header("Default Icon")]
        [SerializeField] private Sprite _defaultSkillIcon;

        private IAssetLoader _assetLoader;
        private ISkillManager _skillManager;
        private Player _player;
        private SkillDataSO[] _slotSkills = new SkillDataSO[3];
        private int _hoveredSlotIndex = -1;
        private Coroutine _tooltipCoroutine;

        public event Action<int> OnSkillSlotClicked;

        private void Awake()
        {
            SetupSlotButtons();
        }

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
                _skillManager = Services.Get<ISkillManager>();
            }

            if (_tooltipPanel != null)
            {
                _tooltipPanel.SetActive(false);
                
                var canvasGroup = _tooltipPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = _tooltipPanel.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;

                SetupTooltipLayout();
            }

            SetupTooltipTexts();
        }

        private void SetupTooltipLayout()
        {
            var vGroup = _tooltipPanel.GetComponent<VerticalLayoutGroup>();
            if (vGroup == null)
                vGroup = _tooltipPanel.AddComponent<VerticalLayoutGroup>();
            
            vGroup.padding = new RectOffset(20, 20, 15, 15);
            vGroup.spacing = 4;
            vGroup.childAlignment = TextAnchor.UpperLeft;
            vGroup.childForceExpandWidth = false;
            vGroup.childForceExpandHeight = false;
            vGroup.childControlWidth = true;
            vGroup.childControlHeight = true;

            var sizeFitter = _tooltipPanel.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
                sizeFitter = _tooltipPanel.AddComponent<ContentSizeFitter>();
            
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void SetupTooltipTexts()
        {
            if (_tooltipName != null)
            {
                _tooltipName.fontSize = 50;
                _tooltipName.fontStyle = FontStyle.Bold;
                _tooltipName.alignment = TextAnchor.MiddleLeft;
                _tooltipName.horizontalOverflow = HorizontalWrapMode.Wrap;
                _tooltipName.verticalOverflow = VerticalWrapMode.Overflow;
                SetTextWidth(_tooltipName, 400);
            }

            if (_tooltipDesc != null)
            {
                _tooltipDesc.fontSize = 30;
                _tooltipDesc.alignment = TextAnchor.MiddleLeft;
                _tooltipDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
                _tooltipDesc.verticalOverflow = VerticalWrapMode.Overflow;
                SetTextWidth(_tooltipDesc, 400);
            }

            if (_tooltipCooldown != null)
            {
                _tooltipCooldown.fontSize = 30;
                _tooltipCooldown.alignment = TextAnchor.MiddleLeft;
                _tooltipCooldown.horizontalOverflow = HorizontalWrapMode.Wrap;
                _tooltipCooldown.verticalOverflow = VerticalWrapMode.Overflow;
                SetTextWidth(_tooltipCooldown, 400);
            }
        }

        private void SetTextWidth(Text text, float width)
        {
            var rectTransform = text.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.sizeDelta = new Vector2(width, 0);
            }
        }

        private void Update()
        {
            UpdateHoverFromMousePosition();
            
            if (_hoveredSlotIndex >= 0 && _tooltipPanel != null && _tooltipPanel.activeSelf)
            {
                UpdateTooltipPositionFromMouse();
            }
        }

        private void UpdateHoverFromMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current == null) return;
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif
            int newSlotIndex = GetSlotIndexFromScreenPoint(mousePos);
            
            if (newSlotIndex >= 0 && newSlotIndex != _hoveredSlotIndex && _slotSkills[newSlotIndex] != null)
            {
                _hoveredSlotIndex = newSlotIndex;
                ShowTooltipFromMouse(_slotSkills[newSlotIndex], mousePos);
            }
            else if (newSlotIndex < 0 && _hoveredSlotIndex >= 0)
            {
                HideTooltip();
                _hoveredSlotIndex = -1;
            }
        }

        private int GetSlotIndexFromScreenPoint(Vector2 screenPoint)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return -1;

            var slots = new[] { _slot1, _slot2, _slot3 };
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                
                RectTransform slotRect = slots[i].GetComponent<RectTransform>();
                if (slotRect == null) continue;

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    slotRect, screenPoint, canvas.worldCamera, out localPoint);

                if (slotRect.rect.Contains(localPoint))
                    return i;
            }
            return -1;
        }

        private void SetupSlotButtons()
        {
            if (_slot1 != null)
                _slot1.onClick.AddListener(() => OnSlotClicked(0));
            if (_slot2 != null)
                _slot2.onClick.AddListener(() => OnSlotClicked(1));
            if (_slot3 != null)
                _slot3.onClick.AddListener(() => OnSlotClicked(2));
        }

        public void SetPlayer(Player player)
        {
            if (_player != null)
            {
                _player.OnSkillsChanged -= UpdateSlots;
            }
            
            _player = player;
            
            if (_player != null)
            {
                _player.OnSkillsChanged += UpdateSlots;
            }
            
            UpdateSlots();
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnSkillsChanged -= UpdateSlots;
            }
        }

        public void UpdateSlots()
        {
            if (_player == null) return;

            SkillDataSO weaponSkill = _player.GetEquippedSkill(EquipmentType.Weapon);
            SkillDataSO armorSkill = _player.GetEquippedSkill(EquipmentType.Armor);
            SkillDataSO accessorySkill = _player.GetEquippedSkill(EquipmentType.Accessory);
            
            UpdateSlot(_slot1, 0, weaponSkill);
            UpdateSlot(_slot2, 1, armorSkill);
            UpdateSlot(_slot3, 2, accessorySkill);
        }

        private async void UpdateSlot(Button slot, int index, SkillDataSO skill)
        {
            _slotSkills[index] = skill;

            if (slot == null) return;

            Image iconImage = GetIconImage(index);

            if (skill == null)
            {
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.color = new Color(1f, 1f, 1f, 0f);
                }
                slot.interactable = false;
                return;
            }

            slot.interactable = true;

            if (iconImage != null)
            {
                iconImage.color = Color.white;
                
                if (_assetLoader == null && Services.IsInitialized)
                {
                    _assetLoader = Services.Get<IAssetLoader>();
                }

                bool iconLoaded = false;
                if (_assetLoader != null && !string.IsNullOrEmpty(skill.IconAddress))
                {
                    try
                    {
                        var sprite = await _assetLoader.LoadAssetAsync<Sprite>(skill.IconAddress);
                        if (sprite != null)
                        {
                            iconImage.sprite = sprite;
                            iconLoaded = true;
                        }
                    }
                    catch
                    {
                    }
                }

                if (!iconLoaded)
                {
                    if (_defaultSkillIcon != null)
                        iconImage.sprite = _defaultSkillIcon;
                    else
                        iconImage.color = new Color(0.5f, 0.5f, 0.8f, 1f);
                }
            }
        }

        private Image GetIconImage(int index)
        {
            return index switch
            {
                0 => _icon1,
                1 => _icon2,
                2 => _icon3,
                _ => null
            };
        }

        private void OnSlotClicked(int index)
        {
            if (_slotSkills[index] != null)
            {
                OnSkillSlotClicked?.Invoke(_slotSkills[index].ID);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_slot1 != null && _slotSkills[0] != null) _slot1.interactable = interactable;
            if (_slot2 != null && _slotSkills[1] != null) _slot2.interactable = interactable;
            if (_slot3 != null && _slotSkills[2] != null) _slot3.interactable = interactable;
        }

        public void UpdateCooldownDisplay()
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            int slotIndex = GetSlotIndexFromEvent(eventData);
            if (slotIndex < 0 || _slotSkills[slotIndex] == null) return;
            
            if (_hoveredSlotIndex != slotIndex)
            {
                _hoveredSlotIndex = slotIndex;
                ShowTooltip(_slotSkills[slotIndex], eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(CheckSlotAfterExit(eventData));
        }

        private IEnumerator CheckSlotAfterExit(PointerEventData eventData)
        {
            yield return null;
            
            int newSlotIndex = GetSlotIndexFromEvent(eventData);
            
            if (newSlotIndex >= 0 && newSlotIndex != _hoveredSlotIndex && _slotSkills[newSlotIndex] != null)
            {
                _hoveredSlotIndex = newSlotIndex;
                ShowTooltip(_slotSkills[newSlotIndex], eventData);
            }
            else if (newSlotIndex < 0)
            {
                HideTooltip();
                _hoveredSlotIndex = -1;
            }
        }

        private bool IsOverAnySlot(PointerEventData eventData)
        {
            if (eventData.pointerEnter == null) return false;
            
            var current = eventData.pointerEnter.transform;
            while (current != null)
            {
                if (current == _slot1?.transform || current == _slot2?.transform || current == _slot3?.transform)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private int GetSlotIndexFromEvent(PointerEventData eventData)
        {
            if (eventData.pointerEnter == null) return -1;
            
            if (eventData.pointerEnter == _slot1?.gameObject) return 0;
            if (eventData.pointerEnter == _slot2?.gameObject) return 1;
            if (eventData.pointerEnter == _slot3?.gameObject) return 2;

            var parent = eventData.pointerEnter.transform.parent;
            if (parent == _slot1?.transform) return 0;
            if (parent == _slot2?.transform) return 1;
            if (parent == _slot3?.transform) return 2;

            return -1;
        }

        private void ShowTooltip(SkillDataSO skill, PointerEventData eventData)
        {
            if (_tooltipPanel == null || skill == null) return;

            if (_tooltipName != null)
                _tooltipName.text = skill.Name;

            if (_tooltipDesc != null)
                _tooltipDesc.text = GetSkillDescription(skill);

            if (_tooltipCooldown != null && _skillManager != null)
            {
                int remaining = _skillManager.GetRemainingCooldown(skill.ID);
                _tooltipCooldown.text = $"쿨타임: {remaining}/{skill.Cooldown}";
            }

            UpdateTooltipPosition(eventData);
            _tooltipPanel.SetActive(true);
        }

        private void ShowTooltipFromMouse(SkillDataSO skill, Vector2 screenPosition)
        {
            if (_tooltipPanel == null || skill == null) return;

            if (_tooltipName != null)
                _tooltipName.text = skill.Name;

            if (_tooltipDesc != null)
                _tooltipDesc.text = GetSkillDescription(skill);

            if (_tooltipCooldown != null && _skillManager != null)
            {
                int remaining = _skillManager.GetRemainingCooldown(skill.ID);
                _tooltipCooldown.text = $"쿨타임: {remaining}/{skill.Cooldown}";
            }

            SetTooltipPosition(screenPosition);
            _tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            if (_tooltipPanel != null)
                _tooltipPanel.SetActive(false);
        }

        private void UpdateTooltipPosition(PointerEventData eventData)
        {
            SetTooltipPosition(eventData.position);
        }

        private void UpdateTooltipPositionFromMouse()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current == null) return;
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif
            SetTooltipPosition(mousePos);
        }

        private void SetTooltipPosition(Vector2 screenPosition)
        {
            if (_tooltipPanel == null) return;

            RectTransform tooltipRect = _tooltipPanel.GetComponent<RectTransform>();
            RectTransform parentRect = _tooltipPanel.transform.parent as RectTransform;
            Canvas canvas = GetComponentInParent<Canvas>();

            if (parentRect == null || canvas == null) return;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPosition,
                canvas.worldCamera,
                out localPos
            );

            Vector2 targetPos = localPos + _tooltipOffset;

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            
            Vector2 tooltipSize = tooltipRect.rect.size;
            Vector2 parentSize = parentRect.rect.size;

            float minX = -parentSize.x / 2f + tooltipSize.x / 2f;
            float maxX = parentSize.x / 2f - tooltipSize.x / 2f;
            float minY = -parentSize.y / 2f + tooltipSize.y / 2f;
            float maxY = parentSize.y / 2f - tooltipSize.y / 2f;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            tooltipRect.anchoredPosition = targetPos;
        }

        private string GetSkillDescription(SkillDataSO skill)
        {
            if (skill == null) return "";

            var desc = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(skill.Description))
            {
                desc.Append(skill.Description);
            }
            else
            {
                desc.Append(GetDefaultDescription(skill));
            }

            if (skill.MPCost > 0)
                desc.Append($"\nMP 소모: {skill.MPCost}");

            return desc.ToString();
        }

        private string GetDefaultDescription(SkillDataSO skill)
        {
            return skill.EffectType switch
            {
                EffectType.Damage => $"적에게 {skill.EffectValue * 100:F0}% 데미지",
                EffectType.Buff => skill.Name.Contains("회복") ? "HP를 회복합니다" : "스탯이 증가합니다",
                EffectType.Passive => "패시브 효과",
                _ => ""
            };
        }
    }
}