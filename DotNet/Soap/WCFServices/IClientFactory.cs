namespace WCFServices
{
    public interface IClientFactory
    {
        public string OwnerType { get; set; }
        public IClient GetInstance(int clientTimeout = 0);
    }
}
