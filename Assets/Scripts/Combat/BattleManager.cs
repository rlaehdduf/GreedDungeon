using System;
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Skill;
using UnityEngine;

namespace GreedDungeon.Combat
{
    public interface IBattleManager
    {
        event Action<Monster> OnBattleStarted;
        event Action<Monster, int> OnMonsterDamaged;
        event Action<string> OnBattleLog;
        
        void StartBattle(Player player, Monster monster);
        void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill);
        void ExecuteDefend(IBattleEntity defender);
        bool ExecuteItem(InventoryItem item, IBattleEntity target);
        void EndTurn();
        bool IsBattleOver { get; }
        bool PlayerWon { get; }
    }

    public class BattleManager : IBattleManager
    {
        private readonly IDamageCalculator _damageCalculator;
        private readonly ITurnManager _turnManager;
        private readonly IGameDataManager _gameDataManager;

        private Player _player;
        private Monster _monster;
        private Monster _previousMonster;
        private List<IBattleEntity> _battleEntities;

        public bool IsBattleOver => _player.IsDead || _monster.IsDead;
        public bool PlayerWon => _monster.IsDead && !_player.IsDead;

        public event Action<Monster> OnBattleStarted;
        public event Action<Monster, int> OnMonsterDamaged;
        public event Action<string> OnBattleLog;

        public BattleManager(IDamageCalculator damageCalculator, ITurnManager turnManager, IGameDataManager gameDataManager)
        {
            _damageCalculator = damageCalculator;
            _turnManager = turnManager;
            _gameDataManager = gameDataManager;
        }

        private void LogBattle(string message)
        {
            Debug.Log(message);
            OnBattleLog?.Invoke(message);
        }

        public void StartBattle(Player player, Monster monster)
        {
            CleanupPreviousBattle();
            
            _player = player;
            _monster = monster;
            _monster.InitializeForBattle();

            _battleEntities = new List<IBattleEntity> { _player, _monster };
            _turnManager.Initialize(_battleEntities);

            _monster.OnDamaged += HandleMonsterDamaged;
            OnBattleStarted?.Invoke(_monster);

            LogBattle($"전투 시작! {_monster.Name} vs Player");
        }

        private void HandleMonsterDamaged(int damage)
        {
            OnMonsterDamaged?.Invoke(_monster, damage);
        }

        private void CleanupPreviousBattle()
        {
            if (_monster != null)
            {
                _monster.OnDamaged -= HandleMonsterDamaged;
            }
        }

        public void ExecuteAttack(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            if (attacker.IsDead || defender.IsDead) return;

            string attackerName = attacker == _player ? "플레이어" : attacker.Name;
            string skillName = skill != null ? $"[{skill.Name}]" : "[기본공격]";
            
            LogBattle($"[공격] {attackerName} → {defender.Name} {skillName}");

            if (skill != null && skill.MPCost > 0)
            {
                if (attacker.CurrentMP < skill.MPCost)
                {
                    LogBattle($"  [실패] MP 부족! (필요: {skill.MPCost}, 현재: {attacker.CurrentMP})");
                    return;
                }
                attacker.UseMP(skill.MPCost);
                LogBattle($"  MP 소모: {skill.MPCost} → {attacker.CurrentMP}/{attacker.TotalStats.MaxMP}");
            }

            int hitCount = skill?.HitCount ?? 1;
            
            for (int hit = 0; hit < hitCount; hit++)
            {
                if (defender.IsDead) break;
                
                if (hitCount > 1)
                {
                    LogBattle($"  ── {hit + 1}타 ──");
                }

                var result = _damageCalculator.CalculateDamage(attacker, defender, skill);

                LogBattle($"  [데미지 계산]");
                LogBattle($"    기본 공격력: {result.BaseAttack}");
                LogBattle($"    스킬 배율: x{result.SkillMultiplier}");
                LogBattle($"    기본 데미지: {result.BaseDamage}");
                LogBattle($"    방어력 감소: -{result.Defense / 2} (DEF: {result.Defense})");
                LogBattle($"    방어 후 데미지: {result.DamageAfterDefense}");

                if (result.IsCritical)
                {
                    LogBattle($"    ★ 크리티컬! x{result.CriticalMultiplier}");
                }

                if (result.ElementMultiplier != 1f)
                {
                    string elementText = result.ElementMultiplier > 1 ? "약점 공격!" : "저항당함...";
                    LogBattle($"    속성 [{result.AttackElement} → {result.DefenderElement}]: x{result.ElementMultiplier} ({elementText})");
                }

                if (result.IsDefending)
                {
                    LogBattle($"    ■ 방어 태세! 데미지 {result.DefenseMultiplier * 100}%로 감소");
                }

                LogBattle($"    ▶ 최종 데미지: {result.Damage}");

                defender.TakeDamage(result.Damage);
            }

            ApplyStatusEffectFromSkill(skill, defender);

            if (defender.IsDead)
            {
                CheckBattleEnd();
            }
        }

        public void ExecuteDefend(IBattleEntity defender)
        {
            if (defender.IsDead) return;

            string defenderName = defender == _player ? "플레이어" : defender.Name;
            defender.StartDefend();
            LogBattle($"[방어] {defenderName}이(가) 방어 태세를 취함");
        }

        public bool ExecuteItem(InventoryItem item, IBattleEntity target)
        {
            if (item == null || item.Type != ItemType.Consumable) return false;
            if (item.Quantity <= 0) return false;
            if (target == null || target.IsDead) return false;

            var data = item.Consumable;
            LogBattle($"[아이템] {data.Name} 사용 → {target.Name}");

            switch (data.EffectType)
            {
                case ConsumableEffectType.Heal:
                    int healAmount = (int)data.EffectValue;
                    target.Heal(healAmount);
                    LogBattle($"  HP 회복: +{healAmount} → {target.CurrentHP}/{target.TotalStats.MaxHP}");
                    break;

                case ConsumableEffectType.Cleanse:
                    target.ClearDebuffs();
                    break;

                case ConsumableEffectType.Buff:
                    if (data.BuffType != BuffType.None)
                    {
                        target.ApplyBuff(data.BuffType, data.EffectValue, data.Duration);
                    }
                    break;

                case ConsumableEffectType.Poison:
                case ConsumableEffectType.Burn:
                    var effect = FindStatusEffect(data.EffectType == ConsumableEffectType.Poison ? "Poison" : "Burn");
                    if (effect != null)
                    {
                        target.ApplyStatusEffect(effect, data.Duration);
                        LogBattle($"  ★ {target.Name}이(가) [{effect.Name}] 상태이상에 걸림!");
                    }
                    break;

                case ConsumableEffectType.Attack:
                    int damage = (int)data.EffectValue;
                    target.TakeDamage(damage);
                    LogBattle($"  데미지: {damage} → HP: {target.CurrentHP}/{target.TotalStats.MaxHP}");
                    if (target.IsDead)
                    {
                        CheckBattleEnd();
                    }
                    break;
            }

            _player.DecrementItemQuantity(item);
            
            return true;
        }

        private void ApplyStatusEffectFromSkill(SkillDataSO skill, IBattleEntity target)
        {
            if (skill == null || string.IsNullOrEmpty(skill.StatusEffectID)) return;

            float roll = UnityEngine.Random.value * 100;
            LogBattle($"  [상태이상 시도] {skill.StatusEffectID} (확률: {skill.StatusEffectChance}%, 주사위: {roll:F1}%)");

            if (roll > skill.StatusEffectChance)
            {
                LogBattle($"  상태이상 저항 성공!");
                return;
            }

            var effect = FindStatusEffect(skill.StatusEffectID);
            if (effect != null)
            {
                int duration = skill.Duration > 0 ? skill.Duration : effect.Duration;
                target.ApplyStatusEffect(effect, duration);
                LogBattle($"  ★ {target.Name}이(가) [{effect.Name}] 상태이상에 걸림! (지속: {duration}턴)");
                if (effect.SkipTurn)
                    LogBattle($"    효과: 턴 스킵");
                if (effect.DamagePerTurn > 0)
                    LogBattle($"    효과: 턴당 {effect.DamagePerTurn} 고정 데미지");
                if (effect.DamageCurrentPercent > 0)
                    LogBattle($"    효과: 현재 HP의 {effect.DamageCurrentPercent * 100}% 데미지");
            }
            else
            {
                LogBattle($"  [경고] StatusEffect '{skill.StatusEffectID}'을(를) 찾을 수 없음");
            }
        }

        private StatusEffectDataSO FindStatusEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId) || _gameDataManager == null) return null;
            if (int.TryParse(effectId, out int id))
            {
                return _gameDataManager.GetStatusEffectData(id);
            }
            return null;
        }

        public void EndTurn()
        {
            var current = _turnManager.CurrentEntity;
            current?.ProcessTurnEnd();

            if (Services.IsInitialized)
            {
                var skillManager = Services.Get<ISkillManager>();
                skillManager?.ReduceAllCooldowns();
            }

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

            LogBattle("═══════════════════════════════════════════════════════════");
            LogBattle("                    전투 종료                              ");
            LogBattle("═══════════════════════════════════════════════════════════");

            if (PlayerWon)
            {
                _player.AddKill();
                int goldReward = _monster.GoldDrop;
                _player.AddGold(goldReward);
                LogBattle($"  결과: 승리!");
                LogBattle($"  레벨: {_player.Level} (처치: {_player.KillCount}/{_player.GetExpRequiredForNextLevel()})");
                LogBattle($"  획득 골드: {goldReward}G");
                LogBattle($"  총 골드: {_player.Gold}G");
            }
            else
            {
                LogBattle($"  결과: 패배...");
                LogBattle($"  레벨: {_player.Level}");
            }
            LogBattle("═══════════════════════════════════════════════════════════");
        }
    }
}