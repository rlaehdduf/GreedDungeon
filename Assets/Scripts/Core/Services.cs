using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public static class Services
    {
        private static DIContainer _container;
        
        public static bool IsInitialized => _container != null;

        public static void Initialize(DIContainer container)
        {
            _container = container;
        }

        public static T Get<T>() where T : class
        {
            if (_container == null)
            {
                return null;
            }
            return _container.Resolve<T>();
        }
    }
}