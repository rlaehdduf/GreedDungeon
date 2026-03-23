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
            
            _battleManager.OnBattleStarted += HandleBattleStarted;
            _battleManager.OnMonsterDamaged += HandleMonsterDamaged;
        }

        public void StartTestBattle()
        {
            if (_gameDataManager == null) return;
            
            var monsterData = _gameDataManager.GetMonsterData(1);
            if (monsterData == null) return;
            
            _currentMonster = new Monster(monsterData);
            _testPlayer = new Player();
            
            _battleManager.StartBattle(_testPlayer, _currentMonster);
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

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnBattleStarted -= HandleBattleStarted;
                _battleManager.OnMonsterDamaged -= HandleMonsterDamaged;
            }
        }
    }
}