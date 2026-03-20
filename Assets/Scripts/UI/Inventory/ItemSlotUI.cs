using System;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Items;

namespace GreedDungeon.UI.Inventory
{
    public class ItemSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _quantityText;
        [SerializeField] private Button _button;

        public event Action OnClicked;

        private ConsumableItem _item;

        private void Start()
        {
            if (_button != null)
                _button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void Setup(ConsumableItem item)
        {
            _item = item;
            
            if (_nameText != null)
                _nameText.text = item.Data.Name;
            
            if (_quantityText != null)
                _quantityText.text = $"x{item.Quantity}";
        }
    }
}