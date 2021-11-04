using DndBoard.SeleniumTestsBase.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsWasm.Fixtures
{
    [CollectionDefinition(nameof(StartServerCollectionWasm))]
    public sealed class StartServerCollectionWasm : ICollectionFixture<StartServerFixtureWasm> { }


    public sealed class StartServerFixtureWasm : StartServerFixtureBase
    {
        protected override string ServerProjDir => "../../../../../../Src/DndBoard/WasmServer";
        protected override int StartTimeoutSec => 30;
        protected override string Host => "localhost";
        protected override int Port => 5001;
    }
}
