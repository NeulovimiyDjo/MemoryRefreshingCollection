using NUnit.Framework;

namespace MyProjectTests.Tests2
{
    [SetUpFixture]
    [SingleThreaded]
    [Parallelizable(ParallelScope.Self)]
    [TestCasesCountInNamespaceBasedOrder(typeof(Tests2SetupFixture))]
    public sealed class Tests2SetupFixture : BaseSetupFixture
    {
        public static DbSetupManager DbSetupManager { get; } = new DbSetupManager(nameof(Tests2SetupFixture));
        public Tests2SetupFixture() : base(DbSetupManager) { }
    }
}
