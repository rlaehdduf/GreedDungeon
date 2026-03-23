using System;
using System.Threading.Tasks;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private Button _slot1;
        [SerializeField] private Button _slot2;
        [SerializeField] private Button _slot3;

        [Header("Cooldown Texts")]
        [SerializeField] private Text _cooldownText1;
        [SerializeField] private Text _cooldownText2;
        [SerializeField] private Text _cooldownText3;

        private IAssetLoader _assetLoader;
        private ISkillManager _skillManager;
        private Player _player;
        private SkillDataSO[] _slotSkills = new SkillDataSO[3];

        public event Action<int> OnSkillSlotClicked;

        private void Awake()
        {
            SetupSlotButtons();
        }

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
                _skillManager = Services.Get<ISkillManager>();
            }
        }

        private void SetupSlotButtons()
        {
            if (_slot1 != null)
                _slot1.onClick.AddListener(() => OnSlotClicked(0));
            if (_slot2 != null)
                _slot2.onClick.AddListener(() => OnSlotClicked(1));
            if (_slot3 != null)
                _slot3.onClick.AddListener(() => OnSlotClicked(2));
        }

        public void SetPlayer(Player player)
        {
            _player = player;
            UpdateSlots();
        }

        public void UpdateSlots()
        {
            if (_player == null) return;

            var skills = _player.Skills;
            
            UpdateSlot(_slot1, 0, skills.Count > 0 ? skills[0] : null);
            UpdateSlot(_slot2, 1, skills.Count > 1 ? skills[1] : null);
            UpdateSlot(_slot3, 2, skills.Count > 2 ? skills[2] : null);
        }

        private async void UpdateSlot(Button slot, int index, SkillDataSO skill)
        {
            _slotSkills[index] = skill;

            if (slot == null) return;

            var image = slot.GetComponent<Image>();
            if (image == null) return;

            if (skill == null)
            {
                image.sprite = null;
                image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                slot.interactable = false;
                UpdateCooldownText(index, 0);
                return;
            }

            slot.interactable = true;
            image.color = Color.white;

            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_assetLoader != null && !string.IsNullOrEmpty(skill.IconAddress))
            {
                var sprite = await _assetLoader.LoadAssetAsync<Sprite>(skill.IconAddress);
                if (sprite != null)
                {
                    image.sprite = sprite;
                }
            }
        }

        private void OnSlotClicked(int index)
        {
            if (_slotSkills[index] != null)
            {
                OnSkillSlotClicked?.Invoke(_slotSkills[index].ID);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_slot1 != null && _slotSkills[0] != null) _slot1.interactable = interactable;
            if (_slot2 != null && _slotSkills[1] != null) _slot2.interactable = interactable;
            if (_slot3 != null && _slotSkills[2] != null) _slot3.interactable = interactable;
        }

        public void UpdateCooldownDisplay()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_slotSkills[i] != null && _skillManager != null)
                {
                    int remaining = _skillManager.GetRemainingCooldown(_slotSkills[i].ID);
                    UpdateCooldownText(i, remaining);
                    
                    if (_skillManager.IsOnCooldown(_slotSkills[i].ID))
                    {
                        SetSlotInteractable(i, false);
                    }
                }
                else
                {
                    UpdateCooldownText(i, 0);
                }
            }
        }

        private void UpdateCooldownText(int index, int remaining)
        {
            Text text = index switch
            {
                0 => _cooldownText1,
                1 => _cooldownText2,
                2 => _cooldownText3,
                _ => null
            };

            if (text != null)
            {
                text.text = remaining > 0 ? remaining.ToString() : "";
                text.gameObject.SetActive(remaining > 0);
            }
        }

        private void SetSlotInteractable(int index, bool interactable)
        {
            Button slot = index switch
            {
                0 => _slot1,
                1 => _slot2,
                2 => _slot3,
                _ => null
            };

            if (slot != null)
                slot.interactable = interactable;
        }
    }
}