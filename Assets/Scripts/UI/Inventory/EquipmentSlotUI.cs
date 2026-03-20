using System;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.UI.Inventory
{
    public class EquipmentSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _typeText;
        [SerializeField] private Text _statsText;
        [SerializeField] private Button _button;

        public event Action OnClicked;

        private EquipmentDataSO _equipment;

        private void Start()
        {
            if (_button != null)
                _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void Setup(EquipmentType type, EquipmentDataSO equipment)
        {
            _equipment = equipment;
            
            if (_typeText != null)
                _typeText.text = type.ToString();
            
            if (equipment == null)
            {
                if (_nameText != null)
                    _nameText.text = "(비어있음)";
                if (_statsText != null)
                    _statsText.text = "";
                return;
            }

            if (_nameText != null)
                _nameText.text = equipment.Name;
            
            if (_statsText != null)
            {
                var stats = new System.Text.StringBuilder();
                if (equipment.HP > 0) stats.Append($"HP+{equipment.HP} ");
                if (equipment.MP > 0) stats.Append($"MP+{equipment.MP} ");
                if (equipment.Attack > 0) stats.Append($"ATK+{equipment.Attack} ");
                if (equipment.Defense > 0) stats.Append($"DEF+{equipment.Defense} ");
                if (equipment.Speed > 0) stats.Append($"SPD+{equipment.Speed} ");
                _statsText.text = stats.ToString().Trim();
            }
        }
    }
}