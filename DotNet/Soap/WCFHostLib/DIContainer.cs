using Unity;
using WCFServices;

namespace WCFHostLib
{
    public static class DIContainer
    {
        public static UnityContainer Container { get; set; }

        static DIContainer()
        {
            Container = new UnityContainer();
            Container.RegisterFactory<IClientFactory>(c => new SingletonPerOwnerTypeAuthClientFactory(), FactoryLifetime.PerResolve);
        }
    }
}
