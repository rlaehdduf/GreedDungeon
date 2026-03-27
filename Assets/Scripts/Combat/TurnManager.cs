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
        event System.Action<IReadOnlyList<IBattleEntity>> OnGaugeUpdated;
    }

    public class TurnManager : ITurnManager
    {
        private const int ACTION_GAUGE_THRESHOLD = 1000;

        private List<IBattleEntity> _entities = new();
        private IBattleEntity _currentEntity;
        private int _turnNumber;
        
        public event System.Action<IReadOnlyList<IBattleEntity>> OnGaugeUpdated;

        public IBattleEntity CurrentEntity => _currentEntity;
        public bool HasNextTurn => _currentEntity != null && !_currentEntity.IsDead;
        public IReadOnlyList<IBattleEntity> TurnOrder => _entities.AsReadOnly();
        public int CurrentTurnNumber => _turnNumber;

        public void Initialize(IEnumerable<IBattleEntity> entities)
        {
            _entities = entities.Where(e => !e.IsDead).ToList();
            _turnNumber = 1;
            
            foreach (var entity in _entities)
            {
                entity.ResetActionGauge();
            }
            
            _currentEntity = null;
            AdvanceToNextActor();
            OnGaugeUpdated?.Invoke(_entities.AsReadOnly());
        }

        private void AdvanceToNextActor()
        {
            while (true)
            {
                foreach (var entity in _entities)
                {
                    if (entity.IsDead) continue;
                    entity.AddActionGauge(entity.TotalStats.Speed);
                }

                var readyEntities = _entities
                    .Where(e => !e.IsDead && e.ActionGauge >= ACTION_GAUGE_THRESHOLD)
                    .OrderByDescending(e => e.ActionGauge + (e.IsPlayer ? 0.1f : 0f))
                    .ThenBy(e => e.Name)
                    .ToList();

                if (readyEntities.Count > 0)
                {
                    _currentEntity = readyEntities[0];
                    return;
                }
            }
        }

        public void NextTurn()
        {
            if (_currentEntity == null) return;
            
            _currentEntity.ResetActionGauge();
            _turnNumber++;
            
            AdvanceToNextActor();
            OnGaugeUpdated?.Invoke(_entities.AsReadOnly());
        }

        public void RemoveEntity(IBattleEntity entity)
        {
            _entities.Remove(entity);
            
            if (_currentEntity == entity)
            {
                _currentEntity = null;
                if (_entities.Any(e => !e.IsDead))
                {
                    AdvanceToNextActor();
                }
            }
        }
    }
}