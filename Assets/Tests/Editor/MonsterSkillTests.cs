using NUnit.Framework;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.Combat;
using GreedDungeon.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GreedDungeon.Tests
{
    [TestFixture]
    public class MonsterSkillTests
    {
        private MonsterDataSO _monsterData;
        private MonsterSkillDataSO _attackSkill;
        private MonsterSkillDataSO _buffSkill;
        private MonsterSkillDataSO _debuffSkill;
        private MonsterSkillDataSO _healSkill;

        [SetUp]
        public void Setup()
        {
            _monsterData = ScriptableObject.CreateInstance<MonsterDataSO>();
            SetField(_monsterData, "ID", 1);
            SetField(_monsterData, "Name", "TestMonster");
            SetField(_monsterData, "MaxHP", 100);
            SetField(_monsterData, "Attack", 20);
            SetField(_monsterData, "Defense", 5);
            SetField(_monsterData, "Speed", 10);
            SetField(_monsterData, "CriticalRate", 5f);
            SetField(_monsterData, "GoldDropMin", 10);
            SetField(_monsterData, "GoldDropMax", 20);
            SetField(_monsterData, "SkillChance", 50f);

            _attackSkill = CreateSkill(1, "강타", MonsterSkillType.Attack, damageMultiplier: 1.5f, hitCount: 2);
            _buffSkill = CreateSkill(2, "분노", MonsterSkillType.Buff, buffType: BuffType.Attack, buffValue: 30f, buffDuration: 3);
            _debuffSkill = CreateSkill(3, "독가스", MonsterSkillType.Debuff, damageMultiplier: 1.0f, statusEffectID: "2", statusEffectChance: 50f);
            _healSkill = CreateSkill(4, "재생", MonsterSkillType.Heal, healPercent: 20f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_monsterData != null) Object.DestroyImmediate(_monsterData);
            if (_attackSkill != null) Object.DestroyImmediate(_attackSkill);
            if (_buffSkill != null) Object.DestroyImmediate(_buffSkill);
            if (_debuffSkill != null) Object.DestroyImmediate(_debuffSkill);
            if (_healSkill != null) Object.DestroyImmediate(_healSkill);
        }

        private MonsterSkillDataSO CreateSkill(int id, string name, MonsterSkillType type,
            float damageMultiplier = 1f, int hitCount = 1,
            string statusEffectID = null, float statusEffectChance = 0f,
            BuffType buffType = BuffType.None, float buffValue = 0f, int buffDuration = 0,
            float healPercent = 0f)
        {
            var skill = ScriptableObject.CreateInstance<MonsterSkillDataSO>();
            SetField(skill, "ID", id);
            SetField(skill, "Name", name);
            SetField(skill, "SkillType", type);
            SetField(skill, "DamageMultiplier", damageMultiplier);
            SetField(skill, "HitCount", hitCount);
            SetField(skill, "StatusEffectID", statusEffectID);
            SetField(skill, "StatusEffectChance", statusEffectChance);
            SetField(skill, "BuffType", buffType);
            SetField(skill, "BuffValue", buffValue);
            SetField(skill, "BuffDuration", buffDuration);
            SetField(skill, "HealPercent", healPercent);
            return skill;
        }

        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }
            
            field = obj.GetType().GetField($"<{fieldName}>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        #region Monster.GetRandomSkill Tests

        [Test]
        public void GetRandomSkill_WhenNoSkills_ReturnsNull()
        {
            var monster = new Monster(_monsterData);
            monster.SetSkills(null, null);

            var result = monster.GetRandomSkill();

            Assert.IsNull(result);
        }

        [Test]
        public void GetRandomSkill_WhenOnlyUniqueSkill_ReturnsSkillOrNull()
        {
            var monster = new Monster(_monsterData);
            monster.SetSkills(_attackSkill, null);

            int skillCount = 0;
            int nullCount = 0;

            for (int i = 0; i < 100; i++)
            {
                var result = monster.GetRandomSkill();
                if (result != null) skillCount++;
                else nullCount++;
            }

            Assert.Greater(skillCount, 0, "스킬이 반환되어야 함");
            Assert.Greater(nullCount, 0, "null도 반환되어야 함 (확률 기반)");
        }

        [Test]
        public void GetRandomSkill_WhenBothSkills_ReturnsEitherOrNull()
        {
            var monster = new Monster(_monsterData);
            monster.SetSkills(_attackSkill, _healSkill);

            var uniqueCount = 0;
            var sharedCount = 0;

            for (int i = 0; i < 100; i++)
            {
                var result = monster.GetRandomSkill();
                if (result == _attackSkill) uniqueCount++;
                else if (result == _healSkill) sharedCount++;
            }

            Assert.Greater(uniqueCount, 0, "전용 스킬이 선택되어야 함");
            Assert.Greater(sharedCount, 0, "공용 스킬이 선택되어야 함");
        }

        [Test]
        public void GetRandomSkill_SkillChanceZero_AlwaysReturnsNull()
        {
            SetField(_monsterData, "SkillChance", 0f);
            var monster = new Monster(_monsterData);
            monster.SetSkills(_attackSkill, _healSkill);

            for (int i = 0; i < 10; i++)
            {
                var result = monster.GetRandomSkill();
                Assert.IsNull(result, "SkillChance 0이면 항상 null이어야 함");
            }
        }

        [Test]
        public void GetRandomSkill_SkillChanceHundred_NeverReturnsNull()
        {
            SetField(_monsterData, "SkillChance", 100f);
            var monster = new Monster(_monsterData);
            monster.SetSkills(_attackSkill, null);

            for (int i = 0; i < 10; i++)
            {
                var result = monster.GetRandomSkill();
                Assert.IsNotNull(result, "SkillChance 100이면 항상 스킬이어야 함");
            }
        }

        #endregion

        #region BattleManager.ExecuteMonsterSkill Tests

        [Test]
        public void ExecuteMonsterSkill_AttackSkill_AppliesCorrectDamage()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            int initialHP = player.CurrentHP;
            battleManager.ExecuteMonsterSkill(_attackSkill);

            int expectedDamage = CalculateExpectedDamage(monster, _attackSkill, player);
            int actualDamage = initialHP - player.CurrentHP;

            Assert.AreEqual(expectedDamage, actualDamage, "공격 스킬 데미지가 올바라야 함");
        }

        [Test]
        public void ExecuteMonsterSkill_BuffSkill_AppliesBuffToMonster()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            monster.InitializeForBattle();
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            battleManager.ExecuteMonsterSkill(_buffSkill);

            Assert.AreEqual(1, monster.Buffs.Count, "버프가 적용되어야 함");
            Assert.AreEqual(BuffType.Attack, monster.Buffs[0].Type);
            Assert.AreEqual(30f, monster.Buffs[0].Value);
            Assert.AreEqual(3, monster.Buffs[0].RemainingDuration);
        }

        [Test]
        public void ExecuteMonsterSkill_HealSkill_HealsMonster()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);
            
            monster.TakeDamage(50);
            int hpBeforeHeal = monster.CurrentHP;

            battleManager.ExecuteMonsterSkill(_healSkill);

            int expectedHeal = Mathf.RoundToInt(monster.BaseStats.MaxHP * 0.2f);
            Assert.AreEqual(hpBeforeHeal + expectedHeal, monster.CurrentHP, "힐 스킬이 HP를 회복해야 함");
        }

        [Test]
        public void ExecuteMonsterSkill_DebuffSkill_AppliesDamage()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            int initialHP = player.CurrentHP;
            battleManager.ExecuteMonsterSkill(_debuffSkill);

            Assert.Less(player.CurrentHP, initialHP, "디버프 스킬이 데미지를 입혀야 함");
        }

        [Test]
        public void ExecuteMonsterSkill_WithPlayerDefending_ReducesDamage()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            player.StartDefend();
            int initialHP = player.CurrentHP;
            battleManager.ExecuteMonsterSkill(_attackSkill);

            int damage = initialHP - player.CurrentHP;
            int expectedDamage = CalculateExpectedDamage(monster, _attackSkill, player, true);

            Assert.AreEqual(expectedDamage, damage, "방어 시 데미지가 절반이어야 함");
        }

        [Test]
        public void ExecuteMonsterSkill_NullSkill_DoesNothing()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            int initialHP = player.CurrentHP;
            battleManager.ExecuteMonsterSkill(null);

            Assert.AreEqual(initialHP, player.CurrentHP, "null 스킬은 아무것도 하지 않아야 함");
        }

        #endregion

        #region Skill Type Tests

        [Test]
        public void AttackSkill_MultipleHits_AppliesDamageForEachHit()
        {
            var multiHitSkill = CreateSkill(10, "연속공격", MonsterSkillType.Attack, damageMultiplier: 0.6f, hitCount: 3);
            var player = new Player();
            var monster = new Monster(_monsterData);
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            int initialHP = player.CurrentHP;
            battleManager.ExecuteMonsterSkill(multiHitSkill);

            int totalDamage = initialHP - player.CurrentHP;
            int expectedPerHit = Mathf.Max(1, Mathf.RoundToInt(20 * 0.6f) - player.TotalStats.Defense / 2);
            int expectedTotal = expectedPerHit * 3;

            Assert.AreEqual(expectedTotal, totalDamage, "다중 타격이 총 데미지에 반영되어야 함");

            Object.DestroyImmediate(multiHitSkill);
        }

        [Test]
        public void BuffSkill_MultipleBuffs_StacksCorrectly()
        {
            var buffSkill1 = CreateSkill(20, "공격증가", MonsterSkillType.Buff, buffType: BuffType.Attack, buffValue: 20f, buffDuration: 2);
            var buffSkill2 = CreateSkill(21, "방어증가", MonsterSkillType.Buff, buffType: BuffType.Defense, buffValue: 30f, buffDuration: 3);

            var player = new Player();
            var monster = new Monster(_monsterData);
            monster.InitializeForBattle();
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            battleManager.ExecuteMonsterSkill(buffSkill1);
            battleManager.ExecuteMonsterSkill(buffSkill2);

            Assert.AreEqual(2, monster.Buffs.Count, "서로 다른 버프 타입은 별도로 적용됨");

            Object.DestroyImmediate(buffSkill1);
            Object.DestroyImmediate(buffSkill2);
        }

        [Test]
        public void HealSkill_AtFullHP_DoesNotExceedMaxHP()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            monster.InitializeForBattle();
            var battleManager = CreateBattleManager();
            battleManager.StartBattle(player, monster);

            int maxHP = monster.BaseStats.MaxHP;
            battleManager.ExecuteMonsterSkill(_healSkill);

            Assert.AreEqual(maxHP, monster.CurrentHP, "최대 HP를 초과해서 회복하지 않아야 함");
        }

        #endregion

        #region Helper Methods

        private BattleManager CreateBattleManager(IGameDataManager gameDataManager = null)
        {
            var damageCalculator = new MockDamageCalculator();
            var turnManager = new MockTurnManager();
            gameDataManager ??= new MockGameDataManager();

            return new BattleManager(damageCalculator, turnManager, gameDataManager);
        }

        private int CalculateExpectedDamage(Monster monster, MonsterSkillDataSO skill, Player player, bool isDefending = false)
        {
            int totalDamage = 0;
            for (int i = 0; i < skill.HitCount; i++)
            {
                int damage = Mathf.RoundToInt(monster.TotalStats.Attack * skill.DamageMultiplier);
                if (isDefending) damage /= 2;
                damage = Mathf.Max(1, damage - player.TotalStats.Defense / 2);
                totalDamage += damage;
            }
            return totalDamage;
        }

        #endregion
    }

    #region Mock Classes

    public class MockDamageCalculator : IDamageCalculator
    {
        public DamageResult CalculateDamage(IBattleEntity attacker, IBattleEntity defender, SkillDataSO skill)
        {
            var result = new DamageResult();
            result.BaseAttack = attacker.TotalStats.Attack;
            result.SkillMultiplier = skill?.EffectValue ?? 1f;
            result.BaseDamage = (int)(result.BaseAttack * result.SkillMultiplier);
            result.Defense = defender.TotalStats.Defense;
            result.DamageAfterDefense = Mathf.Max(1, result.BaseDamage - result.Defense / 2);
            result.IsCritical = false;
            result.CriticalMultiplier = 1f;
            result.ElementMultiplier = 1f;
            result.IsDefending = defender.IsDefending;
            result.DefenseMultiplier = result.IsDefending ? 0.5f : 1f;
            result.Damage = (int)(result.DamageAfterDefense * result.DefenseMultiplier);
            return result;
        }

        public float GetElementMultiplier(Element attacker, Element defender)
        {
            if (attacker == Element.None || defender == Element.None) return 1f;
            return (attacker, defender) switch
            {
                (Element.Fire, Element.Grass) => 1.5f,
                (Element.Water, Element.Fire) => 1.5f,
                (Element.Grass, Element.Water) => 1.5f,
                (Element.Fire, Element.Water) => 0.5f,
                (Element.Water, Element.Grass) => 0.5f,
                (Element.Grass, Element.Fire) => 0.5f,
                _ => 1f
            };
        }
    }

    public class MockTurnManager : ITurnManager
    {
        private List<IBattleEntity> _entities = new();
        private int _currentIndex;

        public IBattleEntity CurrentEntity => _entities.Count > _currentIndex ? _entities[_currentIndex] : null;
        public bool HasNextTurn => _currentIndex < _entities.Count - 1;
        public IReadOnlyList<IBattleEntity> TurnOrder => _entities;
        public int CurrentTurnNumber => 1;

        public void Initialize(IEnumerable<IBattleEntity> entities)
        {
            _entities = new List<IBattleEntity>(entities);
            _currentIndex = 0;
        }

        public void NextTurn()
        {
            _currentIndex = (_currentIndex + 1) % _entities.Count;
        }

        public void RemoveEntity(IBattleEntity entity)
        {
            _entities.Remove(entity);
        }
    }

    public class MockGameDataManager : IGameDataManager
    {
        public bool IsInitialized => true;

        private Dictionary<int, StatusEffectDataSO> _statusEffects = new Dictionary<int, StatusEffectDataSO>();

        public void AddStatusEffect(StatusEffectDataSO effect)
        {
            _statusEffects[effect.ID] = effect;
        }

        public StatusEffectDataSO GetStatusEffectData(int id) =>
            _statusEffects.TryGetValue(id, out var data) ? data : null;

        public PlayerDataSO GetPlayerData() => null;
        public MonsterDataSO GetMonsterData(int id) => null;
        public SkillDataSO GetSkillData(int id) => null;
        public EquipmentDataSO GetEquipmentData(int id) => null;
        public ConsumableDataSO GetConsumableData(int id) => null;
        public RarityDataSO GetRarityData(int id) => null;
        public MonsterSkillDataSO GetMonsterSkillData(int id) => null;
        public IReadOnlyList<MonsterDataSO> GetAllMonsterData() => null;
        public IReadOnlyList<SkillDataSO> GetAllSkillData() => null;
        public IReadOnlyList<EquipmentDataSO> GetAllEquipmentData() => null;
        public IReadOnlyList<ConsumableDataSO> GetAllConsumableData() => null;
        public IReadOnlyList<RarityDataSO> GetAllRarityData() => null;
        public IReadOnlyList<MonsterSkillDataSO> GetAllMonsterSkillData() => null;
        public MonsterDataSO GetRandomMonsterData() => null;
        public MonsterDataSO GetBossMonsterData() => null;
        public System.Threading.Tasks.Task InitializeAsync() => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task<GameObject> LoadMonsterPrefabAsync(string address) => null;
        public System.Threading.Tasks.Task<GameObject> LoadSkillIconAsync(string address) => null;
    }

    #endregion
}