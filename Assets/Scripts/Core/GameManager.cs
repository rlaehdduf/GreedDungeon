using UnityEngine;
using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public class GameManager : IGameManager
    {
        public GameState CurrentState { get; private set; } = GameState.Title;

        [Inject] private ISceneLoader _sceneLoader;

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Title:
                    _sceneLoader.LoadTitle();
                    break;
                case GameState.Dungeon:
                    _sceneLoader.LoadDungeon();
                    break;
                case GameState.Battle:
                    _sceneLoader.LoadBattle();
                    break;
                case GameState.GameOver:
                case GameState.Victory:
                    _sceneLoader.LoadTitle();
                    break;
            }
        }
    }
}