using GreedDungeon.Combat;
using GreedDungeon.Skill;
using Pathfinder.Core;
using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public class GameInstaller : Installer
    {
        public override void Install(DIContainer container)
        {
            Services.Initialize(container);
            
            container.Register<IGameManager, GameManager>(lifetime: ServiceLifetime.Singleton);
            container.Register<ISceneLoader, SceneLoader>(lifetime: ServiceLifetime.Singleton);
            container.Register<IAssetLoader, AddressablesLoader>(lifetime: ServiceLifetime.Singleton);
            container.Register<IGameDataManager, GameDataManager>(lifetime: ServiceLifetime.Singleton);
            
            container.Register<IDamageCalculator, DamageCalculator>(lifetime: ServiceLifetime.Singleton);
            container.Register<ITurnManager, TurnManager>(lifetime: ServiceLifetime.Singleton);
            container.Register<IBattleManager, BattleManager>(lifetime: ServiceLifetime.Singleton);
            container.Register<ISkillManager, SkillManager>(lifetime: ServiceLifetime.Singleton);
        }
    }
}