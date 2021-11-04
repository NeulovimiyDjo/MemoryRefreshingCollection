using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyProjectTests
{
    public abstract class TestBase
    {
        protected readonly DbSetupManager DbSetupManager;

        public TestBase(DbSetupManager dbSetupManager)
        {
            this.DbSetupManager = dbSetupManager;
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDownBase()
        {
            await this.DbSetupManager.ResetDb();
        }
    }
}