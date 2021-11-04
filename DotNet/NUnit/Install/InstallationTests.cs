using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Install
{
    [TestFixture]
    [Order(0)]
    public class InstallationTests
    {
        [Test]
        [Order(0)]
        public async Task Configuration_Imports()
        {
            await InstallSetupFixture.DbSetupManager.CreateDbAndInstall();
        }

        [Test]
        [Order(1)]
        [Category("LongRunning")]
        public async Task ReimportCards_Works()
        {
            await InstallSetupFixture.DbSetupManager.UpdateInstallation();
        }

        [Test]
        [Parallelizable]
        public async Task aSleep_3sec_1()
        {
            await Task.Delay(3000);
        }

        [Test]
        [Parallelizable]
        public async Task Sleep_3sec_2()
        {
            await Task.Delay(3000);
        }
    }
}