using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MyProjectTests
{
    public abstract class BaseSetupFixture
    {
        private DbSetupManager DbSetupManager;

        public BaseSetupFixture(DbSetupManager dbSetupManager)
        {
            DbSetupManager = dbSetupManager;
        }
		

        [OneTimeSetUp]
        public async Task SetUp()
        {
            try
            {
                await DbSetupManager.CreateDbAndInstall();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Unexpected exception during SetUp:" +
                    $"\n-Type: {ex.GetType()}" +
                    $"\n-Message: {ex.Message}" +
                    $"\n-StackTrace:" +
                    $"\n{ex.StackTrace}"
                );
            }
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await DbSetupManager.DeleteDb();
        }
    }
}
