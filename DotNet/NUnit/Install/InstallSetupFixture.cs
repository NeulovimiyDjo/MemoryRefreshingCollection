using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Install
{
    [SetUpFixture]
    [Order(0)]
    [Category("Install")]
    public sealed class InstallSetupFixture
    {
        public static DbSetupManager DbSetupManager { get; } = new DbSetupManager(nameof(InstallSetupFixture));

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await DbSetupManager.DeleteDb();
        }
    }
}
