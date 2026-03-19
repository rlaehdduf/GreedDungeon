using System.Collections.Generic;
using System.Linq;
using GreedDungeon.Character;

namespace GreedDungeon.Combat
{
    public interface ITurnManager
    {
        void Initialize(IEnumerable<IBattleEntity> entities);
        IBattleEntity CurrentEntity { get; }
        bool HasNextTurn { get; }
        void NextTurn();
        void RemoveEntity(IBattleEntity entity);
        IReadOnlyList<IBattleEntity> TurnOrder { get; }
        int CurrentTurnNumber { get; }
    }

    public class TurnManager : ITurnManager
    {
        private List<IBattleEntity> _entities = new();
        private List<IBattleEntity> _turnOrder = new();
        private int _currentIndex;
        private int _turnNumber;

        public IBattleEntity CurrentEntity => _turnOrder.Count > _currentIndex ? _turnOrder[_currentIndex] : null;
        public bool HasNextTurn => _currentIndex < _turnOrder.Count - 1;
        public IReadOnlyList<IBattleEntity> TurnOrder => _turnOrder;
        public int CurrentTurnNumber => _turnNumber;

        public void Initialize(IEnumerable<IBattleEntity> entities)
        {
            _entities = entities.Where(e => !e.IsDead).ToList();
            _turnNumber = 1;
            CalculateTurnOrder();
            _currentIndex = 0;
        }

        private void CalculateTurnOrder()
        {
            _turnOrder = _entities
                .Where(e => !e.IsDead)
                .OrderByDescending(e => e.TotalStats.Speed)
                .ThenBy(e => e.Name)
                .ToList();
        }

        public void NextTurn()
        {
            if (!HasNextTurn)
            {
                StartNewRound();
                return;
            }
            _currentIndex++;
        }

        private void StartNewRound()
        {
            _turnNumber++;
            _currentIndex = 0;
            CalculateTurnOrder();
        }

        public void RemoveEntity(IBattleEntity entity)
        {
            int index = _turnOrder.IndexOf(entity);
            if (index >= 0)
            {
                _turnOrder.RemoveAt(index);
                if (index < _currentIndex)
                    _currentIndex--;
            }
            _entities.Remove(entity);
        }
    }
}