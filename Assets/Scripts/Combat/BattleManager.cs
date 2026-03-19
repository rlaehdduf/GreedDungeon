using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Combat
{
    public interface IBattleManager
    {
        void StartBattle(Player player, Monster monster);
        void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill);
        void EndTurn();
        bool IsBattleOver { get; }
        bool PlayerWon { get; }
    }

    public class BattleManager : IBattleManager
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly ITurnManager _turnManager;

        private Player _player;
        private Monster _monster;
        private List<IBattleEntity> _battleEntities;

        public bool IsBattleOver => _player.IsDead || _monster.IsDead;
        public bool PlayerWon => _monster.IsDead && !_player.IsDead;

        public BattleManager(IDamageCalculator damageCalculator, ITurnManager turnManager)
        {
            _damageCalculator = damageCalculator;
            _turnManager = turnManager;
        }

        public void StartBattle(Player player, Monster monster)
        {
            _player = player;
            _monster = monster;
            _monster.InitializeForBattle();

            _battleEntities = new List<IBattleEntity> { _player, _monster };
            _turnManager.Initialize(_battleEntities);

            Debug.Log($"전투 시작! {_monster.Name} vs Player");
        }

        public void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            if (attacker.IsDead || defender.IsDead) return;

            if (skill != null && skill.MPCost > 0)
            {
                if (attacker.CurrentMP < skill.MPCost)
                {
                    Debug.Log("MP가 부족합니다!");
                    return;
                }
                attacker.UseMP(skill.MPCost);
            }

            var result = _damageCalculator.CalculateDamage(attacker, defender, skill);
            defender.TakeDamage(result.Damage);

            string criticalText = result.IsCritical ? "크리티컬! " : "";
            string elementText = result.ElementMultiplier != 1f ? 
                $" ({(result.ElementMultiplier > 1 ? "약점" : "저항")})" : "";
            Debug.Log($"{criticalText}{defender.Name}에게 {result.Damage} 데미지{elementText}");

            ApplyStatusEffectFromSkill(skill, defender);
        }

        private void ApplyStatusEffectFromSkill(SkillDataSO skill, IBattleEntity target)
        {
            if (skill == null || string.IsNullOrEmpty(skill.StatusEffectID)) return;
            if (Random.value * 100 > skill.StatusEffectChance) return;

            var effect = FindStatusEffect(skill.StatusEffectID);
            if (effect != null)
            {
                target.ApplyStatusEffect(effect, skill.Duration > 0 ? skill.Duration : effect.Duration);
                Debug.Log($"{target.Name}이(가) {effect.Name} 상태이상에 걸림!");
            }
        }

        private StatusEffectDataSO FindStatusEffect(string effectId)
        {
            return null;
        }

        public void EndTurn()
        {
            var current = _turnManager.CurrentEntity;
            current?.ProcessTurnEnd();

            _turnManager.NextTurn();

            var nextEntity = _turnManager.CurrentEntity;
            if (nextEntity != null)
            {
                nextEntity.ProcessTurnStart();
                if (nextEntity.IsDead)
                {
                    CheckBattleEnd();
                }
            }
        }

        private void CheckBattleEnd()
        {
            if (!IsBattleOver) return;

            if (PlayerWon)
            {
                int goldReward = _monster.GoldDrop;
                _player.AddGold(goldReward);
                Debug.Log($"승리! {goldReward} 골드 획득!");
            }
            else
            {
                Debug.Log("패배...");
            }
        }
    }
}