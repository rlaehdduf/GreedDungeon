using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;
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

        public event Action<int> OnSkillSelected;
        public event Action OnAttackClicked;
        public event Action OnDefendClicked;
        public event Action OnItemClicked;

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
            if (_playerStatus != null)
                _playerStatus.Setup(player);

            if (_monsterStatus != null)
                _monsterStatus.Setup(monster);

            if (_battleLog != null)
                _battleLog.Clear();

            if (_skillSlotUI != null)
                _skillSlotUI.SetPlayer(player);
        }

        public void UpdatePlayerStatus(Player player)
        {
            if (_playerStatus != null)
                _playerStatus.UpdateStatus(player);
        }

        public void UpdateMonsterStatus(Monster monster)
        {
            if (_monsterStatus != null)
                _monsterStatus.UpdateStatus(monster);
        }

        public void AddBattleLog(string message)
        {
            if (_battleLog != null)
                _battleLog.AddLog(message);
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
    }
}