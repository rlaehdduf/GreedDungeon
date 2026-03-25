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

        [Header("Debuff (Single Slot)")]
        [SerializeField] private StatusEffectSlotUI _debuffSlot;

        private IAssetLoader _assetLoader;
        private Monster _monster;

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
            if (_debuffSlot != null)
                _debuffSlot.Hide();
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
            UpdateDebuff(monster);
        }

        private void SubscribeMonsterEvents()
        {
            if (_monster == null) return;
            _monster.OnDamaged += OnMonsterDamaged;
            _monster.OnStatusEffectApplied += OnStatusEffectChanged;
            _monster.OnStatusEffectEnded += OnStatusEffectChanged;
            _monster.OnStatusEffectDurationChanged += OnStatusEffectChanged;
        }

        private void UnsubscribeMonsterEvents()
        {
            if (_monster == null) return;
            _monster.OnDamaged -= OnMonsterDamaged;
            _monster.OnStatusEffectApplied -= OnStatusEffectChanged;
            _monster.OnStatusEffectEnded -= OnStatusEffectChanged;
            _monster.OnStatusEffectDurationChanged -= OnStatusEffectChanged;
        }

        private void OnMonsterDamaged(int damage)
        {
            if (_monster != null)
                UpdateStatus(_monster);
        }

        private void OnStatusEffectChanged(IBattleEntity entity, ActiveStatusEffect effect)
        {
            if (_monster != null)
                UpdateDebuff(_monster);
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

        public void UpdateDebuff(Monster monster)
        {
            if (_debuffSlot == null) return;

            if (monster.StatusEffects.Count > 0)
            {
                var effect = monster.StatusEffects[0];
                _debuffSlot.PrepareShow();

                if (effect.Data != null && !string.IsNullOrEmpty(effect.Data.IconAddress))
                {
                    LoadIconAsync(_debuffSlot, effect.Data.IconAddress, effect.RemainingDuration);
                }
                else
                {
                    _debuffSlot.SetDuration(effect.RemainingDuration);
                }
            }
            else
            {
                _debuffSlot.Hide();
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