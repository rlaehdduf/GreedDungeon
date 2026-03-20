using System;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Title
{
    public class TitleUI : UIView
    {
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        public event Action OnStartClicked;
        public event Action OnContinueClicked;
        public event Action OnSettingsClicked;
        public event Action OnCreditsClicked;
        public event Action OnQuitClicked;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(() => OnStartClicked?.Invoke());

            if (_continueButton != null)
                _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());

            if (_creditsButton != null)
                _creditsButton.onClick.AddListener(() => OnCreditsClicked?.Invoke());

            if (_quitButton != null)
                _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void SetContinueEnabled(bool enabled)
        {
            if (_continueButton != null)
                _continueButton.interactable = enabled;
        }
    }
}