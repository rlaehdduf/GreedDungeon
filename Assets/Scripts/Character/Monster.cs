using System;
using GreedDungeon.ScriptableObjects;

namespace GreedDungeon.Character
{
    public class Monster : BattleEntity
    {
        private readonly MonsterDataSO _data;
        private readonly StatusEffectDataSO _statusEffectData;
        private MonsterSkillDataSO _uniqueSkill;
        private MonsterSkillDataSO _sharedSkill;

        public override string Name => _data.Name;
        public MonsterSkillDataSO UniqueSkill => _uniqueSkill;
        public MonsterSkillDataSO SharedSkill => _sharedSkill;
        public float SkillChance => _data.SkillChance;
        public MonsterDataSO Data => _data;
        public Element Element => _data.Element;
        public bool IsBoss => _data.IsBoss;

        public int GoldDrop => UnityEngine.Random.Range(_data.GoldDropMin, _data.GoldDropMax + 1);

        public Monster(MonsterDataSO data, StatusEffectDataSO statusEffectData = null)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _statusEffectData = statusEffectData;

            InitializeStats(new Stats(
                maxHP: data.MaxHP,
                maxMP: 0,
                attack: data.Attack,
                defense: data.Defense,
                speed: data.Speed,
                criticalRate: data.CriticalRate,
                resistance: 0f
            ));
        }

        protected override Stats GetEquipmentStats() => new Stats();

        public void SetSkills(MonsterSkillDataSO unique, MonsterSkillDataSO shared)
        {
            _uniqueSkill = unique;
            _sharedSkill = shared;
        }

        public MonsterSkillDataSO GetRandomSkill()
        {
            if (UnityEngine.Random.value * 100 >= _data.SkillChance)
                return null;
            
            var skills = new System.Collections.Generic.List<MonsterSkillDataSO>();
            if (_uniqueSkill != null) skills.Add(_uniqueSkill);
            if (_sharedSkill != null) skills.Add(_sharedSkill);
            
            return skills.Count > 0 
                ? skills[UnityEngine.Random.Range(0, skills.Count)] 
                : null;
        }

        public bool HasStatusEffectAttack => !string.IsNullOrEmpty(_data.StatusEffectID) && _data.StatusEffectChance > 0;

        public StatusEffectDataSO GetStatusEffectForAttack() => _statusEffectData;

        public float GetStatusEffectChance() => _data.StatusEffectChance;

        public void InitializeForBattle()
        {
            SetInitialHP(BaseStats.MaxHP);
            ClearAllStatusEffects();
        }

        public override void ApplyStatusEffect(StatusEffectDataSO effect, int remainingDuration)
        {
            ClearAllStatusEffects();
            base.ApplyStatusEffect(effect, remainingDuration);
        }
    }
}