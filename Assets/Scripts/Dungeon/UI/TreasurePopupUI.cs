using System;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.UI;
using GreedDungeon.UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Dungeon.UI
{
    public class TreasurePopupUI : UIView
    {
        [Header("UI Elements")]
        [SerializeField] private Text _goldText;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private InventorySlotUI _itemSlotPrefab;
        [SerializeField] private Button _confirmButton;
        
        private Player _player;
        private IGameDataManager _gameDataManager;
        
        public event Action OnConfirmed;
        
        private void Start()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(HandleConfirm);
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _gameDataManager = Services.Get<IGameDataManager>();
        }
        
        public void ShowTreasure(int gold, InventoryItem item)
        {
            if (_goldText != null)
            {
                _goldText.text = $"+{gold}G";
            }
            
            ClearItemSlots();
            
            if (item != null && _itemContainer != null && _itemSlotPrefab != null)
            {
                var slot = Instantiate(_itemSlotPrefab, _itemContainer);
                slot.SetItem(item);
            }
            
            base.Show();
        }
        
        private void ClearItemSlots()
        {
            if (_itemContainer == null) return;
            
            for (int i = _itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemContainer.GetChild(i).gameObject);
            }
        }
        
        private void HandleConfirm()
        {
            Hide();
            OnConfirmed?.Invoke();
        }
    }
}