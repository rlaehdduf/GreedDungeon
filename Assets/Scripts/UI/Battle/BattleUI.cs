using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using System;

namespace GreedDungeon.UI.Battle
{
    public class BattleUI : UIView
    {
        [Header("References")]
        [SerializeField] private PlayerStatusUI _playerStatus;
        [SerializeField] private MonsterStatusUI _monsterStatus;
        [SerializeField] private ActionMenuUI _actionMenu;
        [SerializeField] private BattleLogUI _battleLog;
        [SerializeField] private SkillSlotUI _skillSlotUI;

        [Header("Effects")]
        [SerializeField] private DamageTextUI _damageTextUI;
        [SerializeField] private AttackEffectUI _attackEffectUI;
        [SerializeField] private DebuffVignetteUI _debuffVignetteUI;

        [Header("Inventory")]
        [SerializeField] private Inventory.InventoryUI _inventoryUI;

        public event Action<int> OnSkillSelected;
        public event Action OnAttackClicked;
        public event Action OnDefendClicked;
        public event Action OnItemClicked;
        public event Action<InventoryItem> OnItemUsed;

        private Player _cachedPlayer;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_actionMenu != null)
            {
                _actionMenu.OnAttackClicked += () => OnAttackClicked?.Invoke();
                _actionMenu.OnDefendClicked += () => OnDefendClicked?.Invoke();
                _actionMenu.OnItemClicked += () => OnItemClicked?.Invoke();
            }

            if (_skillSlotUI != null)
            {
                _skillSlotUI.OnSkillSlotClicked += (id) => OnSkillSelected?.Invoke(id);
            }
        }

        public void SetupBattle(Player player, Monster monster)
        {
            _cachedPlayer = player;

            if (_playerStatus != null)
                _playerStatus.Setup(player);

            if (_monsterStatus != null)
                _monsterStatus.Setup(monster);

            if (_battleLog != null)
                _battleLog.Clear();

            if (_skillSlotUI != null)
                _skillSlotUI.SetPlayer(player);

            if (_inventoryUI != null)
            {
                _inventoryUI.Setup(player);
                _inventoryUI.Hide();
                _inventoryUI.OnItemUsed += (item) => OnItemUsed?.Invoke(item);
                _inventoryUI.OnClosed += () => EnableActions(true);
            }

            if (_debuffVignetteUI != null)
            {
                _debuffVignetteUI.Setup(player);
            }
        }

        public void ToggleInventory()
        {
            if (_inventoryUI == null) return;

            if (_inventoryUI.IsOpen)
            {
                _inventoryUI.Hide();
                EnableActions(true);
            }
            else
            {
                _inventoryUI.Show();
                EnableActions(false);
            }
        }

        public void CloseInventory()
        {
            if (_inventoryUI == null || !_inventoryUI.IsOpen) return;
            _inventoryUI.Hide();
            EnableActions(true);
        }

        public void UpdatePlayerStatus(Player player)
        {
            if (_playerStatus != null)
            {
                _playerStatus.UpdateStatus(player);
                _playerStatus.UpdateDebuffs(player);
                _playerStatus.UpdateBuffs(player);
            }
        }

        public void UpdateMonsterStatus(Monster monster)
        {
            if (_monsterStatus != null)
            {
                _monsterStatus.UpdateStatus(monster);
                _monsterStatus.UpdateDebuff(monster);
            }
        }

        public void AddBattleLog(string message, LogType logType = LogType.System)
        {
            if (_battleLog != null)
                _battleLog.AddLog(message, logType);
        }

        public void EnableActions(bool enabled)
        {
            if (_actionMenu != null)
                _actionMenu.EnableButtons(enabled);

            if (_skillSlotUI != null)
                _skillSlotUI.SetInteractable(enabled);
        }

        public void ShowBattleResult(bool playerWon, int goldEarned)
        {
            string message = playerWon 
                ? $"승리! {goldEarned}G 획득" 
                : "패배...";
            AddBattleLog($"═══ {message} ═══");
        }

        public void ShowGameOver()
        {
            AddBattleLog("게임 오버!");
            EnableActions(false);
        }

        public void ShowPlayerDamage(int damage, bool isCritical)
        {
            if (_damageTextUI == null) return;
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(-100, 100),
                UnityEngine.Random.Range(-50, 50)
            );
            _damageTextUI.ShowDamage(damage, randomPos, isCritical, false);
        }

        public void ShowPlayerHeal(int healAmount)
        {
            if (_damageTextUI == null) return;
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(-100, 100),
                UnityEngine.Random.Range(-50, 50)
            );
            _damageTextUI.ShowDamage(healAmount, randomPos, false, true);
        }

        public void ShowMonsterDamage(int damage, bool isCritical)
        {
            if (_damageTextUI == null) return;
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(-50, 50),
                UnityEngine.Random.Range(100, 200)
            );
            _damageTextUI.ShowDamage(damage, randomPos, isCritical, false);
        }

        public void ShowAttackEffect(ScriptableObjects.SkillType skillType)
        {
            if (_attackEffectUI == null) return;
            _attackEffectUI.ShowEffect(skillType);
        }
    }
}