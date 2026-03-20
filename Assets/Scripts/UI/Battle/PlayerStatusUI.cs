using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;

namespace GreedDungeon.UI.Battle
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text _nameText;
        [SerializeField] private Slider _hpBar;
        [SerializeField] private Text _hpText;
        [SerializeField] private Slider _mpBar;
        [SerializeField] private Text _mpText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Transform _statusEffectsContainer;
        [SerializeField] private GameObject _statusEffectPrefab;

        public void Setup(Player player)
        {
            if (_nameText != null)
                _nameText.text = player.Name;

            UpdateStatus(player);
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
    }
}