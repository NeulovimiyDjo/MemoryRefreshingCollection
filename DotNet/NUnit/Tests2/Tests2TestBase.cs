namespace MyProjectTests.Tests2
{
    public abstract class Tests2TestBase : TestBase
    {
        public Tests2TestBase() : base(Tests2SetupFixture.DbSetupManager) { }
    }
}