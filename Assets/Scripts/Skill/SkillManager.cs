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

            Debug.Log($"[SkillManager] Skills loaded: {allSkills.Count}");
            
            foreach (var skill in allSkills)
            {
                var poolType = ConvertSkillTypeToPoolType(skill.Type);
                if (!_skillPools.ContainsKey(poolType))
                    _skillPools[poolType] = new List<SkillDataSO>();
                _skillPools[poolType].Add(skill);
            }

            if (!_skillPools.ContainsKey(SkillPoolType.Random))
                _skillPools[SkillPoolType.Random] = new List<SkillDataSO>(allSkills);

            Debug.Log($"[SkillManager] Skill pool initialization complete: {_skillPools.Count} types");
            foreach (var pool in _skillPools)
            {
                Debug.Log($"  - {pool.Key}: {pool.Value.Count} skills");
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
                Debug.Log($"[SkillManager] {poolType} pool is empty, using Random pool");
                if (_skillPools.TryGetValue(SkillPoolType.Random, out var randomPool) && randomPool.Count > 0)
                    return randomPool[UnityEngine.Random.Range(0, randomPool.Count)];
                Debug.LogWarning($"[SkillManager] All skill pools are empty!");
                return null;
            }
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        public SkillDataSO GetRandomSkill(SkillPoolType poolType, int minTier, int maxTier)
        {
            EnsureInitialized();
            
            if (!_skillPools.TryGetValue(poolType, out var pool) || pool.Count == 0)
            {
                Debug.LogWarning($"[SkillManager] {poolType} pool is empty");
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
                Debug.LogWarning($"[SkillManager] No tier {minTier}-{maxTier} skills in {poolType} pool");
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
            Debug.Log($"[SkillManager] Skill {skillId} cooldown started: {turns} turns");
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
                    Debug.Log($"[SkillManager] Skill {key} cooldown ended");
                }
            }
        }

        public void ResetCooldowns()
        {
            _cooldowns.Clear();
            Debug.Log("[SkillManager] All cooldowns reset");
        }

        public bool ExecuteSkill(SkillDataSO skill, IBattleEntity caster, IBattleEntity target)
        {
            if (skill == null || caster == null) return false;

            Debug.Log($"[SkillManager] Executing skill: {skill.Name}");

            if (skill.MPCost > 0)
            {
                if (caster.CurrentMP < skill.MPCost)
                {
                    Debug.Log($"  Not enough MP! (Need: {skill.MPCost}, Have: {caster.CurrentMP})");
                    return false;
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
            
            return true;
        }

        private void ExecuteBuff(SkillDataSO skill, IBattleEntity caster)
        {
            if (skill.Target == TargetType.Player && skill.EffectType == EffectType.Buff)
            {
                if (skill.Name.Contains("Heal"))
                {
                    int healAmount = (int)(caster.TotalStats.MaxHP * (skill.EffectValue - 1));
                    caster.Heal(healAmount);
                    Debug.Log($"  HP restored: +{healAmount}");
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
            if (skillName.Contains("Attack")) return BuffType.Attack;
            if (skillName.Contains("Defense")) return BuffType.Defense;
            if (skillName.Contains("Speed")) return BuffType.Speed;
            return BuffType.None;
        }

        public void ApplyPassiveStats(SkillDataSO skill, Stats stats)
        {
            if (skill == null || skill.EffectType != EffectType.Passive) return;

            string valueStr = skill.ValueFloat;
            float value = skill.EffectValue;
            bool isInteger = !string.IsNullOrEmpty(valueStr) && valueStr == "i";

            if (skill.Name.Contains("Attack"))
                stats.Attack += isInteger ? (int)value : (int)stats.Attack + (int)value;
            else if (skill.Name.Contains("Defense"))
                stats.Defense += isInteger ? (int)value : (int)stats.Defense + (int)value;
            else if (skill.Name.Contains("HP"))
                stats.MaxHP += isInteger ? (int)value : (int)stats.MaxHP + (int)value;
            else if (skill.Name.Contains("Speed"))
                stats.Speed += isInteger ? (int)value : (int)stats.Speed + (int)value;

            Debug.Log($"[SkillManager] Passive applied: {skill.Name} → stat increase");
        }
    }
}