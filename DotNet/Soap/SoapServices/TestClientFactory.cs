using WCFServices;

namespace SoapServices
{
    public class TestClientFactory : IClientFactory
    {
        public string OwnerType { get; set; }

        public IClient GetInstance()
        {
            return new TestClient();
        }
    }
}
