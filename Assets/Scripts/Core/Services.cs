using Pathfinder.Core.DI;

namespace GreedDungeon.Core
{
    public static class Services
    {
        private static DIContainer _container;

        public static void Initialize(DIContainer container)
        {
            _container = container;
        }

        public static T Get<T>() where T : class
        {
            if (_container == null)
            {
                UnityEngine.Debug.LogError("[Services] 컨테이너가 초기화되지 않았습니다!");
                return null;
            }
            return _container.Resolve<T>();
        }
    }
}