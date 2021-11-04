using DndBoard.SeleniumTestsBase.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsWasm.Fixtures
{
    [CollectionDefinition(nameof(SetupClientsCollectionWasm))]
    public sealed class SetupClientsCollectionWasm : ICollectionFixture<SetupClientsFixtureWasm> { }


    public sealed class SetupClientsFixtureWasm : SetupClientsFixtureBase
    {
        public override string SiteBaseAddress => "https://localhost:5001";
    }
}
