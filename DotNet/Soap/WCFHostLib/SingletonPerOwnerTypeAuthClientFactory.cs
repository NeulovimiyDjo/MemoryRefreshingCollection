using System.Collections.Concurrent;
using WCFServices;

namespace WCFHostLib
{
    public class SingletonPerOwnerTypeAuthClientFactory : IClientFactory
    {
        private static ConcurrentDictionary<string, IClient> _clients
            = new ConcurrentDictionary<string, IClient>();

        public string OwnerType { get; set; }

        public IClient GetInstance(int clientTimeout = 0)
        {
            if (!_clients.ContainsKey(OwnerType))
                _clients[OwnerType] = new AuthClient(clientTimeout);
            return _clients[OwnerType];
        }
    }
}
