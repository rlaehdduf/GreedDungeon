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

        [Header("Skill")]
        [SerializeField] private Image _skillIcon;
        [SerializeField] private GameObject _skillTooltipPanel;
        [SerializeField] private Text _skillTooltipName;
        [SerializeField] private Text _skillTooltipDesc;

        private IAssetLoader _assetLoader;
        private IGameDataManager _gameDataManager;
        private InventoryItem _currentItem;
        private SkillDataSO _cachedSkill;
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
                _gameDataManager = Services.Get<IGameDataManager>();
            }

            if (_skillTooltipPanel != null)
                _skillTooltipPanel.SetActive(false);
        }

        public void Show(InventoryItem item)
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

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void SetName(InventoryItem item)
        {
            if (_nameText == null) return;

            _nameText.text = item.Name;

            if (item.Rarity != null)
            {
                _nameText.color = item.Rarity.Color;
            }
            else
            {
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
            if (item.Type != ItemType.Equipment || item.Equipment == null || item.Rarity == null || !item.Rarity.HasSkill)
            {
                if (_skillIcon != null) _skillIcon.gameObject.SetActive(false);
                if (_skillTooltipPanel != null) _skillTooltipPanel.SetActive(false);
                _cachedSkill = null;
                return;
            }

            if (itemChanged || _cachedSkill == null)
            {
                _cachedSkill = GetRandomSkillForDisplay(item);
            }

            if (_cachedSkill == null)
            {
                if (_skillIcon != null) _skillIcon.gameObject.SetActive(false);
                if (_skillTooltipPanel != null) _skillTooltipPanel.SetActive(false);
                return;
            }

            LoadSkillIcon(_cachedSkill.IconAddress);

            if (_skillTooltipPanel != null)
            {
                _skillTooltipPanel.SetActive(true);

                if (_skillTooltipName != null)
                    _skillTooltipName.text = _cachedSkill.Name;

                if (_skillTooltipDesc != null)
                    _skillTooltipDesc.text = _cachedSkill.Description ?? "";
            }
        }

        private SkillDataSO GetRandomSkillForDisplay(InventoryItem item)
        {
            if (_gameDataManager == null) return null;

            var allSkills = _gameDataManager.GetAllSkillData();
            if (allSkills == null || allSkills.Count == 0) return null;

            int minTier = item.Rarity.SkillTierMin;
            int maxTier = item.Rarity.SkillTierMax;

            var validSkills = new System.Collections.Generic.List<SkillDataSO>();
            foreach (var skill in allSkills)
            {
                if (skill.Tier >= minTier && skill.Tier <= maxTier)
                {
                    validSkills.Add(skill);
                }
            }

            if (validSkills.Count == 0) return null;

            int randomIndex = Random.Range(0, validSkills.Count);
            return validSkills[randomIndex];
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