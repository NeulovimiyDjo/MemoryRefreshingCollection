using DndBoard.SeleniumTestsBase.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsBlazorServer.Fixtures
{
    [CollectionDefinition(nameof(SetupClientsCollectionBlazorServer))]
    public sealed class SetupClientsCollectionBlazorServer : ICollectionFixture<SetupClientsFixtureBlazorServer> { }


    public sealed class SetupClientsFixtureBlazorServer : SetupClientsFixtureBase
    {
        public override string SiteBaseAddress => "https://localhost:5003";
    }
}
