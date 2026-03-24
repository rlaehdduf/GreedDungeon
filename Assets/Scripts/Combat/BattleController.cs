using System;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;

namespace GreedDungeon.Combat
{
    public class BattleController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UI.Battle.MonsterDisplay _monsterDisplay;
        [SerializeField] private UI.Battle.BattleUI _battleUI;

        private IBattleManager _battleManager;
        private IGameDataManager _gameDataManager;
        private ISkillManager _skillManager;
        private Player _testPlayer;
        private Monster _currentMonster;

        public event Action<Monster> OnBattleStarted;
        public event Action<Monster, int> OnMonsterDamaged;
        public event Action OnSkillUsed;

        private void Start()
        {
            StartCoroutine(WaitForDI());
        }

        private System.Collections.IEnumerator WaitForDI()
        {
            while (!Services.IsInitialized)
            {
                yield return null;
            }

            _battleManager = Services.Get<IBattleManager>();
            _gameDataManager = Services.Get<IGameDataManager>();
            _skillManager = Services.Get<ISkillManager>();
            
            if (!_gameDataManager.IsInitialized)
            {
                _gameDataManager.InitializeAsync();
                
                while (!_gameDataManager.IsInitialized)
                {
                    yield return null;
                }
            }
            
            _battleManager.OnBattleStarted += HandleBattleStarted;
            _battleManager.OnMonsterDamaged += HandleMonsterDamaged;

            SetupUIEvents();
        }

        private void SetupUIEvents()
        {
            if (_battleUI != null)
            {
                _battleUI.OnSkillSelected += HandleSkillSelected;
                _battleUI.OnAttackClicked += HandleAttackClicked;
                _battleUI.OnDefendClicked += HandleDefendClicked;
                _battleUI.OnItemClicked += HandleItemClicked;
                _battleUI.OnItemUsed += HandleItemUsed;
            }
            
            StartTestBattle();
        }

        private void HandleSkillSelected(int skillId)
        {
            _battleUI?.AddBattleLog("[플레이어의 턴]");
            
            if (UseSkill(skillId))
            {
                _battleManager.EndTurn();
                _battleUI?.UpdatePlayerStatus(_testPlayer);
                _battleUI?.UpdateMonsterStatus(_currentMonster);
                _skillSlotUI?.UpdateCooldownDisplay();
            }
        }

        private void HandleAttackClicked()
        {
            _battleUI?.AddBattleLog("[플레이어의 턴]");
            
            _battleManager.ExecuteAttack(_testPlayer, _currentMonster, null);
            _battleManager.EndTurn();
            _battleUI?.UpdatePlayerStatus(_testPlayer);
            _battleUI?.UpdateMonsterStatus(_currentMonster);
        }

        private void HandleDefendClicked()
        {
            _battleUI?.AddBattleLog("[플레이어의 턴]");
            
            _battleManager.ExecuteDefend(_testPlayer);
            _battleManager.EndTurn();
            _battleUI?.UpdatePlayerStatus(_testPlayer);
        }

        private void HandleItemClicked()
        {
            _battleUI?.ToggleInventory();
        }

        private void HandleItemUsed(Items.InventoryItem item)
        {
            if (item == null || item.Type != Items.ItemType.Consumable) return;
            
            _battleUI?.AddBattleLog("[플레이어의 턴]");
            
            var target = GetConsumableTarget(item);
            _battleManager.ExecuteItem(item, target);
            _battleManager.EndTurn();
            
            _battleUI?.UpdatePlayerStatus(_testPlayer);
            _battleUI?.UpdateMonsterStatus(_currentMonster);
        }

        private Character.IBattleEntity GetConsumableTarget(Items.InventoryItem item)
        {
            if (item.Consumable == null) return _testPlayer;
            
            return item.Consumable.Target switch
            {
                ConsumableTarget.Player => _testPlayer,
                ConsumableTarget.Single => _currentMonster,
                ConsumableTarget.All => _currentMonster,
                _ => _testPlayer
            };
        }

        private UI.Battle.SkillSlotUI _skillSlotUI;
        private UI.Battle.SkillSlotUI SkillSlotUI
        {
            get
            {
                if (_skillSlotUI == null && _battleUI != null)
                {
                    var field = typeof(UI.Battle.BattleUI).GetField("_skillSlotUI", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _skillSlotUI = field?.GetValue(_battleUI) as UI.Battle.SkillSlotUI;
                }
                return _skillSlotUI;
            }
        }

        public void StartTestBattle()
        {
            if (_gameDataManager == null) return;
            
            var monsterData = _gameDataManager.GetMonsterData(1);
            if (monsterData == null) return;
            
            _currentMonster = new Monster(monsterData);
            _testPlayer = new Player();

            AddTestItemsToInventory();
            
            if (_battleUI != null)
            {
                _battleUI.SetupBattle(_testPlayer, _currentMonster);
            }
            
            _battleManager.StartBattle(_testPlayer, _currentMonster);
        }

        private void AddTestItemsToInventory()
        {
            var allEquipment = _gameDataManager.GetAllEquipmentData();
            if (allEquipment != null)
            {
                foreach (var equipment in allEquipment)
                {
                    _testPlayer.TryAddEquipmentWithHighestRarity(equipment);
                }
            }

            var allConsumables = _gameDataManager.GetAllConsumableData();
            if (allConsumables != null)
            {
                foreach (var consumable in allConsumables)
                {
                    _testPlayer.TryAddConsumable(consumable, 5);
                }
            }

            _testPlayer.AddGold(1000);
        }

        public bool UseSkill(int skillId)
        {
            if (_skillManager == null || _testPlayer == null || _currentMonster == null) return false;
            if (_skillManager.IsOnCooldown(skillId)) return false;

            var skill = _testPlayer.GetSkill(skillId);
            if (skill == null) return false;

            _skillManager.ExecuteSkill(skill, _testPlayer, _currentMonster);
            OnSkillUsed?.Invoke();
            return true;
        }

        public bool IsSkillOnCooldown(int skillId)
        {
            return _skillManager?.IsOnCooldown(skillId) ?? false;
        }

        public int GetSkillCooldown(int skillId)
        {
            return _skillManager?.GetRemainingCooldown(skillId) ?? 0;
        }

        private void HandleBattleStarted(Monster monster)
        {
            _battleManager.OnBattleLog += HandleBattleLog;
            
            OnBattleStarted?.Invoke(monster);
            
            if (_monsterDisplay != null)
            {
                _monsterDisplay.DisplayMonster(monster);
            }
        }

        private void HandleMonsterDamaged(Monster monster, int damage)
        {
            OnMonsterDamaged?.Invoke(monster, damage);
            
            if (_monsterDisplay != null)
            {
                _monsterDisplay.PlayDamageAnimation();
            }
        }

        private void HandleBattleLog(string message)
        {
            if (_battleUI != null)
            {
                _battleUI.AddBattleLog(message);
            }
        }

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnBattleStarted -= HandleBattleStarted;
                _battleManager.OnMonsterDamaged -= HandleMonsterDamaged;
                _battleManager.OnBattleLog -= HandleBattleLog;
            }

            if (_battleUI != null)
            {
                _battleUI.OnSkillSelected -= HandleSkillSelected;
                _battleUI.OnAttackClicked -= HandleAttackClicked;
                _battleUI.OnDefendClicked -= HandleDefendClicked;
                _battleUI.OnItemClicked -= HandleItemClicked;
            }
        }
    }
}