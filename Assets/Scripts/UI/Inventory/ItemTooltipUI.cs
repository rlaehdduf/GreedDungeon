using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Inventory
{
    public class ItemTooltipUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _statsText;
        [SerializeField] private Text _priceText;

        [Header("Skill")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private GameObject _skillTooltipPanel;
        [SerializeField] private Text _skillTooltipName;
        [SerializeField] private Text _skillTooltipDesc;

        private IAssetLoader _assetLoader;
        private InventoryItem _currentItem;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_skillTooltipPanel != null)
                _skillTooltipPanel.SetActive(false);
        }

        public void Show(InventoryItem item)
        {
            Show(item, -1, false);
        }

        public void Show(InventoryItem item, int price, bool isBuying)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            bool itemChanged = _currentItem != item;
            _currentItem = item;

            SetName(item);
            SetDescription(item);
            SetStats(item);
            SetSkillSection(item, itemChanged);
            SetPrice(price, isBuying);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if (_priceText != null)
                _priceText.gameObject.SetActive(false);
        }

        private void SetPrice(int price, bool isBuying)
        {
            if (_priceText == null) return;

            if (price >= 0)
            {
                string prefix = isBuying ? "구매" : "판매";
                _priceText.text = $"{prefix}: {price}G";
                _priceText.color = isBuying ? Color.yellow : Color.green;
                _priceText.gameObject.SetActive(true);
            }
            else
            {
                _priceText.gameObject.SetActive(false);
            }
        }

        private void SetName(InventoryItem item)
        {
            if (_nameText == null) return;

            _nameText.text = item.Name;

            if (item.Rarity != null)
            {
                Debug.Log($"[ItemTooltipUI] SetName: {item.Name}, Rarity: {item.Rarity.Name}, Color: {item.Rarity.Color}");
                _nameText.color = item.Rarity.Color;
            }
            else
            {
                Debug.Log($"[ItemTooltipUI] SetName: {item.Name}, Rarity is null");
                _nameText.color = Color.white;
            }
        }

        private void SetDescription(InventoryItem item)
        {
            if (_descriptionText == null) return;
            _descriptionText.text = item.Description ?? "";
        }

        private void SetStats(InventoryItem item)
        {
            if (_statsText == null) return;

            if (item.Type == ItemType.Consumable)
            {
                SetConsumableStats(item);
            }
            else
            {
                SetEquipmentStats(item);
            }
        }

        private void SetConsumableStats(InventoryItem item)
        {
            var consumable = item.Consumable;
            if (consumable == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"효과: {GetEffectDescription(consumable)}");
            sb.AppendLine($"수량: {item.Quantity}");

            _statsText.text = sb.ToString();
        }

        private string GetEffectDescription(ConsumableDataSO data)
        {
            return data.EffectType switch
            {
                ConsumableEffectType.Heal => $"HP {data.EffectValue:F0} 회복",
                ConsumableEffectType.Cleanse => "디버프 해제",
                ConsumableEffectType.Buff => $"{data.BuffType} +{data.EffectValue:F0}% ({data.Duration}턴)",
                ConsumableEffectType.Poison => $"독 부여 ({data.Duration}턴)",
                ConsumableEffectType.Burn => $"화상 부여 ({data.Duration}턴)",
                ConsumableEffectType.Attack => $"{data.EffectValue:F0} 데미지",
                _ => "알 수 없음"
            };
        }

        private void SetEquipmentStats(InventoryItem item)
        {
            var equipment = item.Equipment;
            if (equipment == null) return;

            var sb = new System.Text.StringBuilder();

            AppendStatLine(sb, "HP", equipment.HP, item.GetBonusHP());
            AppendStatLine(sb, "MP", equipment.MP, item.GetBonusMP());
            AppendStatLine(sb, "ATK", equipment.Attack, item.GetBonusAttack());
            AppendStatLine(sb, "DEF", equipment.Defense, item.GetBonusDefense());
            AppendStatLine(sb, "SPD", equipment.Speed, item.GetBonusSpeed());

            if (equipment.CriticalRate > 0)
            {
                int bonusCrit = (int)item.GetBonusCriticalRate();
                if (bonusCrit > 0)
                    sb.AppendLine($"CRIT +{equipment.CriticalRate:F0}% (+{bonusCrit}%)");
                else
                    sb.AppendLine($"CRIT +{equipment.CriticalRate:F0}%");
            }

            _statsText.text = sb.ToString();
        }

        private void AppendStatLine(System.Text.StringBuilder sb, string statName, int baseValue, int bonusValue)
        {
            if (baseValue <= 0) return;

            if (bonusValue > 0)
                sb.AppendLine($"{statName} +{baseValue} (+{bonusValue})");
            else
                sb.AppendLine($"{statName} +{baseValue}");
        }

        private void SetSkillSection(InventoryItem item, bool itemChanged)
        {
            if (_skillIcon != null) _skillIcon.gameObject.SetActive(false);
            if (_skillTooltipPanel != null) _skillTooltipPanel.SetActive(false);

            if (item.Type != ItemType.Equipment)
            {
                return;
            }

            if (item.Equipment == null)
            {
                return;
            }

            if (item.Rarity == null)
            {
                return;
            }

            if (!item.Rarity.HasSkill)
            {
                return;
            }

            var skill = item.Skill;
            
            if (skill == null)
            {
                if (_skillIcon != null) _skillIcon.gameObject.SetActive(false);
                if (_skillTooltipPanel != null) _skillTooltipPanel.SetActive(false);
                return;
            }

            LoadSkillIcon(skill.IconAddress);

            if (_skillTooltipPanel != null)
            {
                Debug.Log($"[ItemTooltipUI] SkillTooltipPanel 활성화, activeSelf: {_skillTooltipPanel.activeSelf}");
                _skillTooltipPanel.SetActive(true);
                Debug.Log($"[ItemTooltipUI] SkillTooltipPanel 활성화 후 activeSelf: {_skillTooltipPanel.activeSelf}");

                if (_skillTooltipName != null)
                    _skillTooltipName.text = skill.Name;

                if (_skillTooltipDesc != null)
                    _skillTooltipDesc.text = skill.Description ?? "";
            }
            else
            {
                Debug.LogWarning("[ItemTooltipUI] _skillTooltipPanel이 null입니다! Inspector에서 연결해주세요.");
            }
        }

        private async void LoadSkillIcon(string iconAddress)
        {
            if (_skillIcon == null || string.IsNullOrEmpty(iconAddress)) return;

            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_assetLoader == null) return;

            try
            {
                var sprite = await _assetLoader.LoadAssetAsync<Sprite>(iconAddress);
                if (sprite != null)
                {
                    _skillIcon.sprite = sprite;
                    _skillIcon.gameObject.SetActive(true);
                }
                else
                {
                    _skillIcon.gameObject.SetActive(false);
                }
            }
            catch
            {
                _skillIcon.gameObject.SetActive(false);
            }
        }
    }
}