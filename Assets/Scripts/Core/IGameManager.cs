namespace GreedDungeon.Core
{
    public interface IGameManager
    {
        GameState CurrentState { get; }
        void ChangeState(GameState newState);
    }

    public enum GameState
    {
        Title,
        Dungeon,
        Battle,
        GameOver,
        Victory
    }
}