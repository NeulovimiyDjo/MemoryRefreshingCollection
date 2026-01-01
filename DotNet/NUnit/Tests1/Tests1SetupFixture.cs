using NUnit.Framework;

namespace MyProjectTests.Tests1
{
    [SetUpFixture]
    [SingleThreaded]
    [Parallelizable(ParallelScope.Self)]
    [TestCasesCountInNamespaceBasedOrder(typeof(Tests1SetupFixture))]
    public sealed class Tests1SetupFixture : BaseSetupFixture
    {
        public static DbSetupManager DbSetupManager { get; } = new DbSetupManager(nameof(Tests1SetupFixture));
        public Tests1SetupFixture() : base(DbSetupManager) { }
    }
}
