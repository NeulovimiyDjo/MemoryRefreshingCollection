using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Install
{
    [TestFixture]
    [Parallelizable]
    public class InstallationExtraTests
    {
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

        [Test]
        [Parallelizable]
        public async Task IntegrationTestmethod1()
        {
            await Task.Delay(3000);
			// do something with DB
        }
    }
}