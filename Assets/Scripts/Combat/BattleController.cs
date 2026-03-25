using System;
using System.Collections;
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

        [Header("Delay Settings")]
        [SerializeField] private float _attackStartDelay = 0.3f;
        [SerializeField] private float _effectDisplayDelay = 0.5f;
        [SerializeField] private float _afterDamageDelay = 0.5f;
        [SerializeField] private float _turnTransitionDelay = 0.3f;

        private IBattleManager _battleManager;
        private IGameDataManager _gameDataManager;
        private ISkillManager _skillManager;
        private Player _testPlayer;
        private Monster _currentMonster;
        private bool _isActionInProgress;

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
            _battleManager.OnPlayerDeath += HandlePlayerDeath;
            _battleManager.OnMonsterDeath += HandleMonsterDeath;
            _battleManager.OnMonsterTurnStarted += HandleMonsterTurnStarted;
            _battleManager.OnPlayerDamaged += HandlePlayerDamaged;
            _battleManager.OnPlayerHealed += HandlePlayerHealed;
            _battleManager.OnAttackEffect += HandleAttackEffect;

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
            if (_isActionInProgress) return;
            StartCoroutine(HandleSkillSelectedCoroutine(skillId));
        }

        private IEnumerator HandleSkillSelectedCoroutine(int skillId)
        {
            _isActionInProgress = true;
            _battleUI?.EnableActions(false);

            yield return new WaitForSeconds(_attackStartDelay);

            if (UseSkill(skillId))
            {
                yield return new WaitForSeconds(_effectDisplayDelay);
                _battleManager.EndTurn();
                _battleUI?.UpdatePlayerStatus(_testPlayer);
                _battleUI?.UpdateMonsterStatus(_currentMonster);
                _skillSlotUI?.UpdateCooldownDisplay();
            }

            yield return new WaitForSeconds(_turnTransitionDelay);
            _isActionInProgress = false;
            _battleUI?.EnableActions(true);
        }

        private void HandleAttackClicked()
        {
            if (_isActionInProgress) return;
            StartCoroutine(HandleAttackCoroutine());
        }

        private IEnumerator HandleAttackCoroutine()
        {
            _isActionInProgress = true;
            _battleUI?.EnableActions(false);

            yield return new WaitForSeconds(_attackStartDelay);

            _battleManager.ExecuteAttack(_testPlayer, _currentMonster, null);

            yield return new WaitForSeconds(_afterDamageDelay);
            _battleManager.EndTurn();
            _battleUI?.UpdatePlayerStatus(_testPlayer);
            _battleUI?.UpdateMonsterStatus(_currentMonster);

            yield return new WaitForSeconds(_turnTransitionDelay);
            _isActionInProgress = false;
            _battleUI?.EnableActions(true);
        }

        private void HandleDefendClicked()
        {
            if (_isActionInProgress) return;
            StartCoroutine(HandleDefendCoroutine());
        }

        private IEnumerator HandleDefendCoroutine()
        {
            _isActionInProgress = true;
            _battleUI?.EnableActions(false);

            yield return new WaitForSeconds(_attackStartDelay);

            _battleManager.ExecuteDefend(_testPlayer);

            yield return new WaitForSeconds(_afterDamageDelay);
            _battleManager.EndTurn();
            _battleUI?.UpdatePlayerStatus(_testPlayer);

            yield return new WaitForSeconds(_turnTransitionDelay);
            _isActionInProgress = false;
            _battleUI?.EnableActions(true);
        }

        private void HandleMonsterTurnStarted()
        {
            if (_isActionInProgress) return;
            StartCoroutine(HandleMonsterTurnCoroutine());
        }

        private IEnumerator HandleMonsterTurnCoroutine()
        {
            _isActionInProgress = true;
            _battleUI?.EnableActions(false);

            yield return new WaitForSeconds(_attackStartDelay);

            if (_monsterDisplay != null)
            {
                yield return _monsterDisplay.PlayAttackAnimation();
            }

            yield return new WaitForSeconds(_effectDisplayDelay);

            _battleManager.ExecuteMonsterAttack();

            yield return new WaitForSeconds(_afterDamageDelay);

            if (_testPlayer.IsDead)
            {
                _battleUI?.ShowGameOver();
            }
            else
            {
                _battleManager.EndTurn();
                _battleUI?.UpdatePlayerStatus(_testPlayer);
                _battleUI?.UpdateMonsterStatus(_currentMonster);
            }

            yield return new WaitForSeconds(_turnTransitionDelay);
            _isActionInProgress = false;
            _battleUI?.EnableActions(true);
        }

        private void HandleItemClicked()
        {
            _battleUI?.ToggleInventory();
        }

        private void HandleItemUsed(Items.InventoryItem item)
        {
            if (_isActionInProgress) return;
            StartCoroutine(HandleItemUsedCoroutine(item));
        }

        private IEnumerator HandleItemUsedCoroutine(Items.InventoryItem item)
        {
            _isActionInProgress = true;
            _battleUI?.EnableActions(false);

            yield return new WaitForSeconds(_attackStartDelay);

            if (item == null || item.Type != Items.ItemType.Consumable)
            {
                _isActionInProgress = false;
                _battleUI?.EnableActions(true);
                yield break;
            }

            var target = GetConsumableTarget(item);
            _battleManager.ExecuteItem(item, target);

            yield return new WaitForSeconds(_afterDamageDelay);
            _battleManager.EndTurn();

            _battleUI?.CloseInventory();
            _battleUI?.UpdatePlayerStatus(_testPlayer);
            _battleUI?.UpdateMonsterStatus(_currentMonster);

            yield return new WaitForSeconds(_turnTransitionDelay);
            _isActionInProgress = false;
            _battleUI?.EnableActions(true);
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
            if (allEquipment != null && allEquipment.Count > 0)
            {
                _testPlayer.TryAddEquipmentWithHighestRarity(allEquipment[0]);
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

            if (monster != null)
            {
                monster.OnStatusEffectApplied += OnMonsterDebuffChanged;
                monster.OnStatusEffectEnded += OnMonsterDebuffChanged;
            }
        }

        private void OnMonsterDebuffChanged(IBattleEntity entity, ActiveStatusEffect effect)
        {
            if (_monsterDisplay == null || _currentMonster == null) return;

            if (_currentMonster.StatusEffects.Count > 0)
            {
                var debuff = _currentMonster.StatusEffects[0];
                _monsterDisplay.UpdateDebuffColor(debuff.Data?.ID ?? 0);
            }
            else
            {
                _monsterDisplay.ClearDebuffColor();
            }
        }

        private void HandleMonsterDamaged(Monster monster, int damage)
        {
            OnMonsterDamaged?.Invoke(monster, damage);
            
            if (_monsterDisplay != null)
            {
                _monsterDisplay.PlayDamageAnimation();
            }

            _battleUI?.ShowMonsterDamage(damage, false);
        }

        private void HandlePlayerDamaged(int damage, bool isCritical)
        {
            _battleUI?.ShowPlayerDamage(damage, isCritical);
        }

        private void HandlePlayerHealed(int healAmount)
        {
            _battleUI?.ShowPlayerHeal(healAmount);
        }

        private void HandleAttackEffect(ScriptableObjects.SkillType skillType)
        {
            _battleUI?.ShowAttackEffect(skillType);
        }

        private void HandleBattleLog(string message, UI.Battle.LogType logType)
        {
            if (_battleUI != null)
            {
                _battleUI.AddBattleLog(message, logType);
            }
        }

        private void HandlePlayerDeath()
        {
            _battleUI?.ShowGameOver();
        }

        private void HandleMonsterDeath()
        {
        }

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnBattleStarted -= HandleBattleStarted;
                _battleManager.OnMonsterDamaged -= HandleMonsterDamaged;
                _battleManager.OnBattleLog -= HandleBattleLog;
                _battleManager.OnPlayerDeath -= HandlePlayerDeath;
                _battleManager.OnMonsterDeath -= HandleMonsterDeath;
                _battleManager.OnMonsterTurnStarted -= HandleMonsterTurnStarted;
                _battleManager.OnPlayerDamaged -= HandlePlayerDamaged;
                _battleManager.OnPlayerHealed -= HandlePlayerHealed;
                _battleManager.OnAttackEffect -= HandleAttackEffect;
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