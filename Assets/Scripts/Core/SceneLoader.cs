using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public class SceneLoader : ISceneLoader
    {
        private const string TitleScene = "Title";
        private const string DungeonScene = "Dungeon";
        private const string BattleScene = "Battle";

        private string _currentScene = TitleScene;

        public void LoadTitle()
        {
            LoadScene(TitleScene);
        }

        public void LoadDungeon()
        {
            LoadScene(DungeonScene);
        }

        public void LoadBattle()
        {
            LoadScene(BattleScene);
        }

        public void ReloadCurrentScene()
        {
            LoadScene(_currentScene);
        }

        private void LoadScene(string sceneName)
        {
            _currentScene = sceneName;
            SceneManager.LoadScene(sceneName);
        }
    }
}