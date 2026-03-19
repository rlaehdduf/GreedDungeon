using Pathfinder.Core;
using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public class GameInstaller : Installer
    {
        public override void Install(DIContainer container)
        {
            container.Register<IGameManager, GameManager>(lifetime: ServiceLifetime.Singleton);
            container.Register<ISceneLoader, SceneLoader>(lifetime: ServiceLifetime.Singleton);
        }
    }
}