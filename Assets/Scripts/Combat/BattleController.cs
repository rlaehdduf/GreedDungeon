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
            }
        }

        private void HandleSkillSelected(int skillId)
        {
            Debug.Log($"[BattleController] 스킬 선택: {skillId}");
            if (UseSkill(skillId))
            {
                Debug.Log($"[BattleController] 스킬 사용 성공!");
                _battleUI?.UpdatePlayerStatus(_testPlayer);
                _battleUI?.UpdateMonsterStatus(_currentMonster);
                _skillSlotUI?.UpdateCooldownDisplay();
            }
        }

        private void HandleAttackClicked()
        {
            Debug.Log("[BattleController] 공격 버튼 클릭");
            _battleManager.ExecuteAttack(_testPlayer, _currentMonster, null);
            _battleUI?.UpdatePlayerStatus(_testPlayer);
            _battleUI?.UpdateMonsterStatus(_currentMonster);
        }

        private void HandleDefendClicked()
        {
            Debug.Log("[BattleController] 방어 버튼 클릭");
            _battleManager.ExecuteDefend(_testPlayer);
            _battleUI?.UpdatePlayerStatus(_testPlayer);
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

            EquipTestItems();
            
            _battleManager.StartBattle(_testPlayer, _currentMonster);
        }

        private void EquipTestItems()
        {
            var allEquipment = _gameDataManager.GetAllEquipmentData();
            if (allEquipment == null || allEquipment.Count == 0)
            {
                Debug.Log("[BattleController] 장비 데이터 없음");
                return;
            }

            int equippedCount = 0;
            foreach (var equipment in allEquipment)
            {
                if (equipment != null && equippedCount < 3)
                {
                    _testPlayer.Equip(equipment);
                    equippedCount++;
                    Debug.Log($"[BattleController] 장비 장착: {equipment.Name} (Type: {equipment.Type}, SkillPool: {equipment.SkillPoolType})");
                }
            }

            Debug.Log($"[BattleController] 총 {equippedCount}개 장비 장착 완료");
            Debug.Log($"[BattleController] 스킬 개수: {_testPlayer.Skills.Count}");
            foreach (var skill in _testPlayer.Skills)
            {
                Debug.Log($"  - {skill.Name} (Type: {skill.Type}, Effect: {skill.EffectType})");
            }
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

            if (_battleUI != null)
            {
                _battleUI.OnSkillSelected -= HandleSkillSelected;
                _battleUI.OnAttackClicked -= HandleAttackClicked;
                _battleUI.OnDefendClicked -= HandleDefendClicked;
            }
        }
    }
}