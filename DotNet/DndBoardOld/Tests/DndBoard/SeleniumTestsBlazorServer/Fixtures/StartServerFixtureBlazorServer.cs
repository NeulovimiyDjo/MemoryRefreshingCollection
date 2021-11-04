using DndBoard.SeleniumTestsBase.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsBlazorServer.Fixtures
{
    [CollectionDefinition(nameof(StartServerCollectionBlazorServer))]
    public sealed class StartServerCollectionBlazorServer : ICollectionFixture<StartServerFixtureBlazorServer> { }


    public sealed class StartServerFixtureBlazorServer : StartServerFixtureBase
    {
        protected override string ServerProjDir => "../../../../../../Src/DndBoard/BlazorServer";
        protected override int StartTimeoutSec => 30;
        protected override string Host => "localhost";
        protected override int Port => 5003;
    }
}
