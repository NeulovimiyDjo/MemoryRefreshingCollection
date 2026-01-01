namespace WCFServices
{
    public class WCFServiceBase
    {
        protected readonly IClientFactory ClientFactory;

        public WCFServiceBase(IClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
            ClientFactory.OwnerType = this.GetType().FullName;
        }
    }
}
