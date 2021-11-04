namespace MyProjectTests.Tests1
{
    public abstract class Tests1TestBase : TestBase
    {
        public Tests1TestBase() : base(Tests1SetupFixture.DbSetupManager) { }
    }
}