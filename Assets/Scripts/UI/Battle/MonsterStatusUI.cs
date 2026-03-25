using System.Collections.Generic;
using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;

namespace GreedDungeon.UI.Battle
{
    public class MonsterStatusUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text _nameText;
        [SerializeField] private Slider _hpBar;
        [SerializeField] private Text _hpText;
        [SerializeField] private Image _elementIcon;

        [Header("Status Effects (Debuffs)")]
        [SerializeField] private Transform _debuffContainer;
        [SerializeField] private GameObject _debuffSlotPrefab;

        private IAssetLoader _assetLoader;
        private Monster _monster;
        private readonly List<StatusEffectSlotUI> _debuffSlots = new();

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeMonsterEvents();
        }

        public void Setup(Monster monster)
        {
            UnsubscribeMonsterEvents();
            _monster = monster;
            SubscribeMonsterEvents();

            if (_nameText != null)
                _nameText.text = monster.Name;

            LoadElementIcon(monster.Element);
            UpdateStatus(monster);
            UpdateDebuffs(monster);
        }

        private void SubscribeMonsterEvents()
        {
            if (_monster == null) return;
            _monster.OnDamaged += OnMonsterDamaged;
            _monster.OnStatusEffectApplied += OnStatusEffectChanged;
            _monster.OnStatusEffectEnded += OnStatusEffectChanged;
        }

        private void UnsubscribeMonsterEvents()
        {
            if (_monster == null) return;
            _monster.OnDamaged -= OnMonsterDamaged;
            _monster.OnStatusEffectApplied -= OnStatusEffectChanged;
            _monster.OnStatusEffectEnded -= OnStatusEffectChanged;
        }

        private void OnMonsterDamaged(int damage)
        {
            if (_monster != null)
                UpdateStatus(_monster);
        }

        private void OnStatusEffectChanged(IBattleEntity entity, ActiveStatusEffect effect)
        {
            if (_monster != null)
                UpdateDebuffs(_monster);
        }

        private async void LoadElementIcon(Element element)
        {
            if (_elementIcon == null) return;
            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
            if (_assetLoader == null) return;

            string address = GetElementAddress(element);
            if (string.IsNullOrEmpty(address))
            {
                _elementIcon.gameObject.SetActive(false);
                return;
            }

            try
            {
                var sprite = await _assetLoader.LoadAssetAsync<Sprite>(address);
                if (sprite != null)
                {
                    _elementIcon.sprite = sprite;
                    _elementIcon.gameObject.SetActive(true);
                }
                else
                {
                    _elementIcon.gameObject.SetActive(false);
                }
            }
            catch
            {
                _elementIcon.gameObject.SetActive(false);
            }
        }

        private string GetElementAddress(Element element)
        {
            return element switch
            {
                Element.Fire => "Elements/Fire",
                Element.Water => "Elements/Water",
                Element.Grass => "Elements/Leaf",
                Element.None => "Elements/Neutral",
                _ => null
            };
        }

        public void UpdateStatus(Monster monster)
        {
            var stats = monster.TotalStats;
            
            if (_hpBar != null)
            {
                _hpBar.maxValue = stats.MaxHP;
                _hpBar.value = monster.CurrentHP;
            }

            if (_hpText != null)
                _hpText.text = $"{monster.CurrentHP}/{stats.MaxHP}";
        }

        public void UpdateDebuffs(Monster monster)
        {
            EnsureSlotCount(_debuffSlots, _debuffContainer, _debuffSlotPrefab, monster.StatusEffects.Count);
            
            for (int i = 0; i < _debuffSlots.Count; i++)
            {
                if (i < monster.StatusEffects.Count)
                {
                    var effect = monster.StatusEffects[i];
                    var slot = _debuffSlots[i];
                    slot.Show();

                    if (effect.Data != null && !string.IsNullOrEmpty(effect.Data.IconAddress))
                    {
                        LoadIconAsync(slot, effect.Data.IconAddress, effect.RemainingDuration);
                    }
                    else
                    {
                        slot.SetDuration(effect.RemainingDuration);
                    }
                }
                else
                {
                    _debuffSlots[i].Hide();
                }
            }
        }

        private void EnsureSlotCount(List<StatusEffectSlotUI> slots, Transform container, GameObject prefab, int requiredCount)
        {
            while (slots.Count < requiredCount)
            {
                var go = Instantiate(prefab, container);
                var slot = go.GetComponent<StatusEffectSlotUI>();
                if (slot != null)
                {
                    slots.Add(slot);
                }
                else
                {
                    Destroy(go);
                    break;
                }
            }
        }

        private async void LoadIconAsync(StatusEffectSlotUI slot, string address, int duration)
        {
            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
            if (_assetLoader == null) return;

            try
            {
                var sprite = await _assetLoader.LoadAssetAsync<Sprite>(address);
                if (sprite != null && slot != null)
                {
                    slot.SetIcon(sprite, duration);
                }
            }
            catch
            {
            }
        }
    }
}