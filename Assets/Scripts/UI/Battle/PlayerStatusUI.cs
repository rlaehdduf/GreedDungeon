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
        [SerializeField] private List<StatusEffectSlotUI> _debuffSlots = new();

        [Header("Buffs")]
        [SerializeField] private List<StatusEffectSlotUI> _buffSlots = new();

        private IAssetLoader _assetLoader;

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
            ClearAllSlots();
        }

        public void Setup(Player player)
        {
            UpdateStatus(player);
            UpdateDebuffs(player);
            UpdateBuffs(player);
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
            int index = 0;
            foreach (var effect in player.StatusEffects)
            {
                if (index >= _debuffSlots.Count) break;

                var slot = _debuffSlots[index];
                slot.Show();

                if (effect.Data != null && !string.IsNullOrEmpty(effect.Data.IconAddress))
                {
                    LoadIconAsync(slot, effect.Data.IconAddress, effect.RemainingDuration);
                }
                else
                {
                    slot.SetDuration(effect.RemainingDuration);
                }
                index++;
            }

            for (int i = index; i < _debuffSlots.Count; i++)
            {
                _debuffSlots[i].Hide();
            }
        }

        public void UpdateBuffs(Player player)
        {
            int index = 0;
            foreach (var buff in player.Buffs)
            {
                if (index >= _buffSlots.Count) break;

                var slot = _buffSlots[index];
                slot.Show();

                string iconAddress = buff.GetIconAddress();
                if (!string.IsNullOrEmpty(iconAddress))
                {
                    LoadIconAsync(slot, iconAddress, buff.RemainingDuration);
                }
                else
                {
                    slot.SetDuration(buff.RemainingDuration);
                }
                index++;
            }

            for (int i = index; i < _buffSlots.Count; i++)
            {
                _buffSlots[i].Hide();
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

        private void ClearAllSlots()
        {
            foreach (var slot in _debuffSlots)
            {
                slot.Hide();
            }
            foreach (var slot in _buffSlots)
            {
                slot.Hide();
            }
        }
    }
}