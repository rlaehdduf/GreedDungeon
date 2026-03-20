using System;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Popup
{
    public class ConfirmPopup : UIView
    {
        [Header("UI Elements")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Text _confirmText;
        [SerializeField] private Text _cancelText;

        public event Action OnConfirmed;
        public event Action OnCancelled;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(() => 
                {
                    OnConfirmed?.Invoke();
                    Hide();
                });

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(() => 
                {
                    OnCancelled?.Invoke();
                    Hide();
                });
        }

        public void Show(string title, string message, string confirmLabel = "확인", string cancelLabel = "취소")
        {
            if (_titleText != null)
                _titleText.text = title;

            if (_messageText != null)
                _messageText.text = message;

            if (_confirmText != null)
                _confirmText.text = confirmLabel;

            if (_cancelText != null)
                _cancelText.text = cancelLabel;

            base.Show();
        }

        public void ShowConfirmOnly(string title, string message, string confirmLabel = "확인")
        {
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(false);

            Show(title, message, confirmLabel);
        }

        public override void Hide()
        {
            base.Hide();
            
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(true);
        }
    }
}