using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;

namespace GreedDungeon.UI.Battle
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _hpBar;
        [SerializeField] private Text _hpText;
        [SerializeField] private Slider _mpBar;
        [SerializeField] private Text _mpText;
        [Header("Status Effects")]
        [SerializeField] private Transform _statusEffectsContainer;
        [SerializeField] private GameObject _statusEffectPrefab;
        [SerializeField] private Transform _buffsContainer;
        [SerializeField] private GameObject _buffPrefab;

        private readonly List<GameObject> _statusEffectIcons = new();
        private readonly List<GameObject> _buffIcons = new();

        public void Setup(Player player)
        {
            UpdateStatus(player);
            UpdateStatusEffects(player);
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

        public void UpdateStatusEffects(Player player)
        {
            ClearIcons(_statusEffectIcons);

            if (_statusEffectsContainer == null || _statusEffectPrefab == null) return;

            foreach (var effect in player.StatusEffects)
            {
                var icon = Instantiate(_statusEffectPrefab, _statusEffectsContainer);
                var texts = icon.GetComponentsInChildren<Text>();
                
                foreach (var text in texts)
                {
                    if (text.name.Contains("Count") || text.name.Contains("Duration"))
                        text.text = effect.RemainingDuration.ToString();
                }
                
                _statusEffectIcons.Add(icon);
            }
        }

        public void UpdateBuffs(Player player)
        {
            ClearIcons(_buffIcons);

            if (_buffsContainer == null || _buffPrefab == null) return;

            foreach (var buff in player.Buffs)
            {
                var icon = Instantiate(_buffPrefab, _buffsContainer);
                var texts = icon.GetComponentsInChildren<Text>();
                
                foreach (var text in texts)
                {
                    if (text.name.Contains("Count") || text.name.Contains("Duration"))
                        text.text = buff.RemainingDuration.ToString();
                    else if (text.name.Contains("Value"))
                        text.text = $"+{buff.Value}%";
                }
                
                _buffIcons.Add(icon);
            }
        }

        private void ClearIcons(List<GameObject> icons)
        {
            foreach (var icon in icons)
            {
                if (icon != null) Destroy(icon);
            }
            icons.Clear();
        }
    }
}