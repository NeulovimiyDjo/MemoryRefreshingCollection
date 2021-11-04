using DndBoard.SeleniumTestsBase;
using DndBoard.SeleniumTestsBlazorServer.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsBlazorServer
{
    [Collection(nameof(StartServerCollectionBlazorServer))]
    public sealed class OverallTestsBlazorServer : OverallTestsBase, IClassFixture<SetupClientsFixtureBlazorServer>
    {
        public OverallTestsBlazorServer(SetupClientsFixtureBlazorServer setup)
            : base(setup) { }
    }
}
