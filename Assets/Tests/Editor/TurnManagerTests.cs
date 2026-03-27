using NUnit.Framework;
using GreedDungeon.Character;
using GreedDungeon.Combat;
using GreedDungeon.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine;

namespace GreedDungeon.Tests
{
    [TestFixture]
    public class TurnManagerTests
    {
        private TurnManager _turnManager;
        private MonsterDataSO _monsterData;

        [SetUp]
        public void Setup()
        {
            _turnManager = new TurnManager();
            _monsterData = ScriptableObject.CreateInstance<MonsterDataSO>();
            SetField(_monsterData, "ID", 1);
            SetField(_monsterData, "Name", "TestMonster");
            SetField(_monsterData, "MaxHP", 100);
            SetField(_monsterData, "Attack", 10);
            SetField(_monsterData, "Defense", 5);
            SetField(_monsterData, "Speed", 10);
            SetField(_monsterData, "CriticalRate", 5f);
            SetField(_monsterData, "GoldDropMin", 10);
            SetField(_monsterData, "GoldDropMax", 20);
            SetField(_monsterData, "SkillChance", 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_monsterData != null) Object.DestroyImmediate(_monsterData);
        }

        private void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [Test]
        public void TurnOrder_PlayerAndMonster_AlternatesCorrectly()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var entities = new List<IBattleEntity> { player, monster };

            _turnManager.Initialize(entities);

            List<string> turnOrder = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                turnOrder.Add(_turnManager.CurrentEntity.Name);
                _turnManager.NextTurn();
            }

            Assert.AreEqual(4, turnOrder.Count, "4턴이 기록되어야 함");
            
            if (player.TotalStats.Speed >= monster.TotalStats.Speed)
            {
                Assert.AreEqual("Player", turnOrder[0], "첫 번째는 Player");
                Assert.AreEqual("TestMonster", turnOrder[1], "두 번째는 Monster");
                Assert.AreEqual("Player", turnOrder[2], "세 번째는 Player (새 라운드)");
                Assert.AreEqual("TestMonster", turnOrder[3], "네 번째는 Monster");
            }
        }

        [Test]
        public void NextTurn_NoDoubleTurn_ForSameEntity()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var entities = new List<IBattleEntity> { player, monster };

            _turnManager.Initialize(entities);

            var first = _turnManager.CurrentEntity;
            _turnManager.NextTurn();
            var second = _turnManager.CurrentEntity;
            _turnManager.NextTurn();
            var third = _turnManager.CurrentEntity;

            Assert.AreNotSame(first, second, "연속으로 같은 엔티티가 아니어야 함");
            Assert.AreSame(first, third, "세 번째는 첫 번째와 같아야 함 (새 라운드)");
        }

        [Test]
        public void EndTurn_Sequence_NoSkip()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var entities = new List<IBattleEntity> { player, monster };

            _turnManager.Initialize(entities);

            int playerTurnCount = 0;
            int monsterTurnCount = 0;

            for (int i = 0; i < 10; i++)
            {
                var current = _turnManager.CurrentEntity;
                if (current == player as IBattleEntity) playerTurnCount++;
                else if (current == monster as IBattleEntity) monsterTurnCount++;
                
                _turnManager.NextTurn();
            }

            Assert.AreEqual(5, playerTurnCount, "Player는 5턴을 가져야 함");
            Assert.AreEqual(5, monsterTurnCount, "Monster는 5턴을 가져야 함");
        }

        [Test]
        public void CurrentTurnNumber_IncrementsEachRound()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var entities = new List<IBattleEntity> { player, monster };

            _turnManager.Initialize(entities);

            Assert.AreEqual(1, _turnManager.CurrentTurnNumber, "시작 턴 번호는 1");

            _turnManager.NextTurn();
            Assert.AreEqual(1, _turnManager.CurrentTurnNumber, "같은 라운드 내에서는 턴 번호 유지");

            _turnManager.NextTurn();
            Assert.AreEqual(2, _turnManager.CurrentTurnNumber, "새 라운드에서 턴 번호 증가");
        }

        [Test]
        public void HasNextTurn_ReturnsCorrectValue()
        {
            var player = new Player();
            var monster = new Monster(_monsterData);
            var entities = new List<IBattleEntity> { player, monster };

            _turnManager.Initialize(entities);

            Assert.IsTrue(_turnManager.HasNextTurn, "첫 번째 엔티티에서는 HasNextTurn이 true");

            _turnManager.NextTurn();
            Assert.IsFalse(_turnManager.HasNextTurn, "마지막 엔티티에서는 HasNextTurn이 false");

            _turnManager.NextTurn();
            Assert.IsTrue(_turnManager.HasNextTurn, "새 라운드 시작 시 HasNextTurn이 true");
        }
    }
}