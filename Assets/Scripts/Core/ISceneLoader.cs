namespace GreedDungeon.Core
{
    public interface ISceneLoader
    {
        void LoadTitle();
        void LoadDungeon();
        void LoadBattle();
        void ReloadCurrentScene();
    }
}