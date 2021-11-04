using DndBoard.SeleniumTestsBase;
using DndBoard.SeleniumTestsWasm.Fixtures;
using Xunit;

namespace DndBoard.SeleniumTestsWasm
{
    [Collection(nameof(StartServerCollectionWasm))]
    public sealed class OverallTestsWasm : OverallTestsBase, IClassFixture<SetupClientsFixtureWasm>
    {
        public OverallTestsWasm(SetupClientsFixtureWasm setup)
            : base(setup) { }
    }
}
