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
        [SerializeField] private Text _levelText;
        [SerializeField] private Image _elementIcon;
        [SerializeField] private Transform _statusEffectsContainer;
        [SerializeField] private GameObject _statusEffectPrefab;

        public void Setup(Monster monster)
        {
            if (_nameText != null)
                _nameText.text = monster.Name;

            UpdateStatus(monster);
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
    }
}