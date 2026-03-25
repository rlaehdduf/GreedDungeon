using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Combat;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Skill
{
    public class SkillManager : ISkillManager
    {
        private readonly IGameDataManager _gameDataManager;
        
        private readonly Dictionary<SkillPoolType, List<SkillDataSO>> _skillPools = new();
        private readonly Dictionary<int, int> _cooldowns = new();
        private bool _isInitialized;

        public SkillManager(IGameDataManager gameDataManager)
        {
            _gameDataManager = gameDataManager;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            if (_gameDataManager == null || !_gameDataManager.IsInitialized) return;
            
            _isInitialized = true;
            InitializeSkillPools();
        }

        private void InitializeSkillPools()
        {
            var allSkills = _gameDataManager.GetAllSkillData();
            if (allSkills == null)
            {
                Debug.LogWarning("[SkillManager] GetAllSkillData() returned null");
                return;
            }

            Debug.Log($"[SkillManager] 로드된 스킬 수: {allSkills.Count}");
            
            foreach (var skill in allSkills)
            {
                var poolType = ConvertSkillTypeToPoolType(skill.Type);
                if (!_skillPools.ContainsKey(poolType))
                    _skillPools[poolType] = new List<SkillDataSO>();
                _skillPools[poolType].Add(skill);
            }

            if (!_skillPools.ContainsKey(SkillPoolType.Random))
                _skillPools[SkillPoolType.Random] = new List<SkillDataSO>(allSkills);

            Debug.Log($"[SkillManager] 스킬 풀 초기화 완료: {_skillPools.Count}개 타입");
            foreach (var pool in _skillPools)
            {
                Debug.Log($"  - {pool.Key}: {pool.Value.Count}개 스킬");
            }
        }

        private SkillPoolType ConvertSkillTypeToPoolType(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Neutral => SkillPoolType.Neutral,
                SkillType.Melee => SkillPoolType.Melee,
                SkillType.Magic => SkillPoolType.Magic,
                SkillType.Passive => SkillPoolType.Passive,
                SkillType.Buff => SkillPoolType.Buff,
                _ => SkillPoolType.Neutral
            };
        }

        public SkillDataSO GetRandomSkill(SkillPoolType poolType)
        {
            EnsureInitialized();
            
            if (!_skillPools.TryGetValue(poolType, out var pool) || pool.Count == 0)
            {
                Debug.Log($"[SkillManager] {poolType} 풀이 비어있음, Random 풀 사용");
                if (_skillPools.TryGetValue(SkillPoolType.Random, out var randomPool) && randomPool.Count > 0)
                    return randomPool[UnityEngine.Random.Range(0, randomPool.Count)];
                Debug.LogWarning($"[SkillManager] 모든 스킬 풀이 비어있음!");
                return null;
            }
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        public SkillDataSO GetRandomSkill(SkillPoolType poolType, int minTier, int maxTier)
        {
            EnsureInitialized();
            
            if (!_skillPools.TryGetValue(poolType, out var pool) || pool.Count == 0)
            {
                Debug.LogWarning($"[SkillManager] {poolType} 풀이 비어있음");
                return null;
            }

            var filteredSkills = new List<SkillDataSO>();
            foreach (var skill in pool)
            {
                if (skill.Tier >= minTier && skill.Tier <= maxTier)
                    filteredSkills.Add(skill);
            }

            if (filteredSkills.Count == 0)
            {
                Debug.LogWarning($"[SkillManager] {poolType} 풀에 tier {minTier}-{maxTier} 스킬 없음");
                return null;
            }

            return filteredSkills[UnityEngine.Random.Range(0, filteredSkills.Count)];
        }

        public bool IsOnCooldown(int skillId)
        {
            return _cooldowns.TryGetValue(skillId, out var remaining) && remaining > 0;
        }

        public int GetRemainingCooldown(int skillId)
        {
            return _cooldowns.TryGetValue(skillId, out var remaining) ? remaining : 0;
        }

        public void StartCooldown(int skillId, int turns)
        {
            if (turns <= 0) return;
            _cooldowns[skillId] = turns;
            Debug.Log($"[SkillManager] 스킬 {skillId} 쿨다운 시작: {turns}턴");
        }

        public void ReduceAllCooldowns()
        {
            var keys = new List<int>(_cooldowns.Keys);
            foreach (var key in keys)
            {
                _cooldowns[key]--;
                if (_cooldowns[key] <= 0)
                {
                    _cooldowns.Remove(key);
                    Debug.Log($"[SkillManager] 스킬 {key} 쿨다운 종료");
                }
            }
        }

        public void ResetCooldowns()
        {
            _cooldowns.Clear();
            Debug.Log("[SkillManager] 모든 쿨다운 초기화");
        }

        public void ExecuteSkill(SkillDataSO skill, IBattleEntity caster, IBattleEntity target)
        {
            if (skill == null || caster == null) return;

            Debug.Log($"[SkillManager] 스킬 실행: {skill.Name}");

            if (skill.MPCost > 0)
            {
                if (caster.CurrentMP < skill.MPCost)
                {
                    Debug.Log($"  MP 부족! (필요: {skill.MPCost}, 현재: {caster.CurrentMP})");
                    return;
                }
                caster.UseMP(skill.MPCost);
            }

            switch (skill.EffectType)
            {
                case EffectType.Damage:
                    if (target != null && Services.IsInitialized)
                    {
                        var battleManager = Services.Get<IBattleManager>();
                        battleManager?.ExecuteAttack(caster, target, skill);
                    }
                    break;

                case EffectType.Buff:
                    ExecuteBuff(skill, caster);
                    break;
            }

            if (skill.Cooldown > 0)
                StartCooldown(skill.ID, skill.Cooldown + 1);
        }

        private void ExecuteBuff(SkillDataSO skill, IBattleEntity caster)
        {
            if (skill.Target == TargetType.Player && skill.EffectType == EffectType.Buff)
            {
                if (skill.Name.Contains("회복"))
                {
                    int healAmount = (int)(caster.TotalStats.MaxHP * (skill.EffectValue - 1));
                    caster.Heal(healAmount);
                    Debug.Log($"  HP 회복: +{healAmount}");
                }
                else
                {
                    BuffType buffType = DetermineBuffType(skill.Name);
                    if (buffType != BuffType.None)
                    {
                        int duration = skill.Duration > 0 ? skill.Duration : 3;
                        caster.ApplyBuff(buffType, skill.EffectValue, duration);
                    }
                }
            }
        }

        private BuffType DetermineBuffType(string skillName)
        {
            if (skillName.Contains("공격력")) return BuffType.Attack;
            if (skillName.Contains("방어력")) return BuffType.Defense;
            if (skillName.Contains("속도")) return BuffType.Speed;
            return BuffType.None;
        }

        public void ApplyPassiveStats(SkillDataSO skill, Stats stats)
        {
            if (skill == null || skill.EffectType != EffectType.Passive) return;

            string valueStr = skill.ValueFloat;
            float value = skill.EffectValue;
            bool isInteger = !string.IsNullOrEmpty(valueStr) && valueStr == "i";

            if (skill.Name.Contains("공격력"))
                stats.Attack += isInteger ? (int)value : (int)stats.Attack + (int)value;
            else if (skill.Name.Contains("방어력"))
                stats.Defense += isInteger ? (int)value : (int)stats.Defense + (int)value;
            else if (skill.Name.Contains("체력"))
                stats.MaxHP += isInteger ? (int)value : (int)stats.MaxHP + (int)value;
            else if (skill.Name.Contains("속도"))
                stats.Speed += isInteger ? (int)value : (int)stats.Speed + (int)value;

            Debug.Log($"[SkillManager] 패시브 적용: {skill.Name} → 스탯 증가");
        }
    }
}