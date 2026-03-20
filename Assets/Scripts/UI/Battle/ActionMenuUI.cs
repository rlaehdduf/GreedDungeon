using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.UI.Battle
{
    public class ActionMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _itemButton;

        [Header("Panels")]
        [SerializeField] private GameObject _skillPanel;
        [SerializeField] private Transform _skillContainer;
        [SerializeField] private GameObject _skillButtonPrefab;

        [SerializeField] private GameObject _itemPanel;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private GameObject _itemButtonPrefab;

        private readonly List<GameObject> _skillButtons = new();
        private readonly List<GameObject> _itemButtons = new();

        public event Action OnAttackClicked;
        public event Action OnDefendClicked;
        public event Action<int> OnSkillSelected;
        public event Action<int> OnItemSelected;

        private void Start()
        {
            SetupMainButtons();
        }

        private void SetupMainButtons()
        {
            if (_attackButton != null)
                _attackButton.onClick.AddListener(() => OnAttackClicked?.Invoke());

            if (_defendButton != null)
                _defendButton.onClick.AddListener(() => OnDefendClicked?.Invoke());

            if (_skillButton != null)
                _skillButton.onClick.AddListener(ShowSkillPanel);

            if (_itemButton != null)
                _itemButton.onClick.AddListener(ShowItemPanel);
        }

        public void Setup(Player player)
        {
            SetupSkills(player);
            SetupItems(player);
        }

        private void SetupSkills(Player player)
        {
            ClearSkillButtons();
            
            if (_skillContainer == null || _skillButtonPrefab == null) return;

            foreach (var skill in player.Skills)
            {
                var go = Instantiate(_skillButtonPrefab, _skillContainer);
                var button = go.GetComponent<Button>();
                var text = go.GetComponentInChildren<Text>();
                
                if (text != null)
                    text.text = skill.Name;
                
                int skillId = skill.ID;
                if (button != null)
                    button.onClick.AddListener(() => OnSkillSelected?.Invoke(skillId));
                
                _skillButtons.Add(go);
            }
        }

        private void SetupItems(Player player)
        {
            ClearItemButtons();
            
            if (_itemContainer == null || _itemButtonPrefab == null) return;

            foreach (var kvp in player.Inventory)
            {
                var item = kvp.Value;
                if (item.Quantity <= 0) continue;

                var go = Instantiate(_itemButtonPrefab, _itemContainer);
                var button = go.GetComponent<Button>();
                var text = go.GetComponentInChildren<Text>();
                
                if (text != null)
                    text.text = $"{item.Data.Name} x{item.Quantity}";
                
                int itemId = item.Data.ID;
                if (button != null)
                    button.onClick.AddListener(() => OnItemSelected?.Invoke(itemId));
                
                _itemButtons.Add(go);
            }
        }

        private void ClearSkillButtons()
        {
            foreach (var go in _skillButtons)
            {
                if (go != null) Destroy(go);
            }
            _skillButtons.Clear();
        }

        private void ClearItemButtons()
        {
            foreach (var go in _itemButtons)
            {
                if (go != null) Destroy(go);
            }
            _itemButtons.Clear();
        }

        public void ShowSkillPanel()
        {
            if (_skillPanel != null)
            {
                _skillPanel.SetActive(true);
                if (_itemPanel != null)
                    _itemPanel.SetActive(false);
            }
        }

        public void ShowItemPanel()
        {
            if (_itemPanel != null)
            {
                _itemPanel.SetActive(true);
                if (_skillPanel != null)
                    _skillPanel.SetActive(false);
            }
        }

        public void HidePanels()
        {
            if (_skillPanel != null) _skillPanel.SetActive(false);
            if (_itemPanel != null) _itemPanel.SetActive(false);
        }

        public void EnableButtons(bool enabled)
        {
            if (_attackButton != null) _attackButton.interactable = enabled;
            if (_defendButton != null) _defendButton.interactable = enabled;
            if (_skillButton != null) _skillButton.interactable = enabled;
            if (_itemButton != null) _itemButton.interactable = enabled;
        }
    }
}