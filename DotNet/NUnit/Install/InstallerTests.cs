using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Install
{
    [TestFixture]
    [Parallelizable]
    public class InstallerTests
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
        public async Task ResetDb_Works()
        {
            string getCountQuery = "select count(*) from SomeTable with(nolock)";

            int beforeDeletion = await Execute(getCountQuery);
            await Exectute("delete from SomeTable");
            int afterDeletion = await Execute(getCountQuery);
            Assert.AreEqual(0, afterDeletion);

            await InstallSetupFixture.DbSetupManager.ResetDb();
            int afterReset = await Execute(getCountQuery);
            Assert.AreEqual(beforeDeletion, afterReset);
        }
    }
}