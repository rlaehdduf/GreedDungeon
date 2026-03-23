using System;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Inventory
{
    public class ConfirmDropPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _confirmButton;

        public event Action OnConfirmed;

        private void Start()
        {
            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(Hide);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            gameObject.SetActive(false);
        }

        public void Show(string itemName)
        {
            if (_messageText != null)
                _messageText.text = $"{itemName}을(를) 버리시겠습니까?";

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            OnConfirmed?.Invoke();
            Hide();
        }
    }
}