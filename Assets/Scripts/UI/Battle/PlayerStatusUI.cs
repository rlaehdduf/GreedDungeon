using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;
using GreedDungeon.Core;

namespace GreedDungeon.UI.Battle
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _hpBar;
        [SerializeField] private Text _hpText;
        [SerializeField] private Slider _mpBar;
        [SerializeField] private Text _mpText;

        [Header("Status Effects (Debuffs)")]
        [SerializeField] private Transform _debuffContainer;
        [SerializeField] private GameObject _debuffSlotPrefab;

        [Header("Buffs")]
        [SerializeField] private Transform _buffContainer;
        [SerializeField] private GameObject _buffSlotPrefab;

        private IAssetLoader _assetLoader;
        private Player _player;
        private readonly List<StatusEffectSlotUI> _debuffSlots = new();
        private readonly List<StatusEffectSlotUI> _buffSlots = new();

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
        }

        private void OnDestroy()
        {
            UnsubscribePlayerEvents();
        }

        public void Setup(Player player)
        {
            UnsubscribePlayerEvents();
            _player = player;
            SubscribePlayerEvents();
            UpdateStatus(player);
            UpdateDebuffs(player);
            UpdateBuffs(player);
        }

        private void SubscribePlayerEvents()
        {
            if (_player == null) return;
            _player.OnDamaged += OnPlayerDamaged;
            _player.OnStatsChanged += OnStatsChanged;
            _player.OnStatusEffectApplied += OnStatusEffectChanged;
            _player.OnStatusEffectEnded += OnStatusEffectChanged;
            _player.OnStatusEffectDurationChanged += OnStatusEffectChanged;
            _player.OnBuffApplied += OnBuffChanged;
            _player.OnBuffEnded += OnBuffChanged;
            _player.OnBuffDurationChanged += OnBuffChanged;
        }

        private void UnsubscribePlayerEvents()
        {
            if (_player == null) return;
            _player.OnDamaged -= OnPlayerDamaged;
            _player.OnStatsChanged -= OnStatsChanged;
            _player.OnStatusEffectApplied -= OnStatusEffectChanged;
            _player.OnStatusEffectEnded -= OnStatusEffectChanged;
            _player.OnStatusEffectDurationChanged -= OnStatusEffectChanged;
            _player.OnBuffApplied -= OnBuffChanged;
            _player.OnBuffEnded -= OnBuffChanged;
            _player.OnBuffDurationChanged -= OnBuffChanged;
        }

        private void OnPlayerDamaged(int damage)
        {
            if (_player != null)
                UpdateStatus(_player);
        }

        private void OnStatsChanged()
        {
            if (_player != null)
                UpdateStatus(_player);
        }

        private void OnStatusEffectChanged(IBattleEntity entity, ActiveStatusEffect effect)
        {
            if (_player != null)
                UpdateDebuffs(_player);
        }

        private void OnBuffChanged(IBattleEntity entity, ActiveBuff buff)
        {
            if (_player != null)
            {
                UpdateBuffs(_player);
                UpdateStatus(_player);
            }
        }

        public void UpdateStatus(Player player)
        {
            var stats = player.TotalStats;
            
            if (_hpBar != null)
            {
                _hpBar.maxValue = stats.MaxHP;
                _hpBar.value = player.CurrentHP;
            }

            if (_hpText != null)
                _hpText.text = $"{player.CurrentHP}/{stats.MaxHP}";

            if (_mpBar != null)
            {
                _mpBar.maxValue = stats.MaxMP;
                _mpBar.value = player.CurrentMP;
            }

            if (_mpText != null)
                _mpText.text = $"{player.CurrentMP}/{stats.MaxMP}";
        }

        public void UpdateDebuffs(Player player)
        {
            EnsureSlotCount(_debuffSlots, _debuffContainer, _debuffSlotPrefab, player.StatusEffects.Count);
            
            for (int i = 0; i < _debuffSlots.Count; i++)
            {
                if (i < player.StatusEffects.Count)
                {
                    var effect = player.StatusEffects[i];
                    var slot = _debuffSlots[i];
                    slot.PrepareShow();

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

        public void UpdateBuffs(Player player)
        {
            EnsureSlotCount(_buffSlots, _buffContainer, _buffSlotPrefab, player.Buffs.Count);
            
            for (int i = 0; i < _buffSlots.Count; i++)
            {
                if (i < player.Buffs.Count)
                {
                    var buff = player.Buffs[i];
                    var slot = _buffSlots[i];
                    slot.PrepareShow();

                    string iconAddress = buff.GetIconAddress();
                    if (!string.IsNullOrEmpty(iconAddress))
                    {
                        LoadIconAsync(slot, iconAddress, buff.RemainingDuration);
                    }
                    else
                    {
                        slot.SetDuration(buff.RemainingDuration);
                    }
                }
                else
                {
                    _buffSlots[i].Hide();
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
                    slot.Hide();
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