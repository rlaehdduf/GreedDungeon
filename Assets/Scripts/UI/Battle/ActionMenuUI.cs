using System;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class ActionMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _itemButton;

        public event Action OnAttackClicked;
        public event Action OnDefendClicked;
        public event Action OnItemClicked;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_attackButton != null)
                _attackButton.onClick.AddListener(() => OnAttackClicked?.Invoke());

            if (_defendButton != null)
                _defendButton.onClick.AddListener(() => OnDefendClicked?.Invoke());

            if (_itemButton != null)
                _itemButton.onClick.AddListener(() => OnItemClicked?.Invoke());
        }

        public void EnableButtons(bool enabled)
        {
            if (_attackButton != null) _attackButton.interactable = enabled;
            if (_defendButton != null) _defendButton.interactable = enabled;
            if (_itemButton != null) _itemButton.interactable = enabled;
        }
    }
}