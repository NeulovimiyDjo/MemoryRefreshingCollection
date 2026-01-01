using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Tests1
{
    [TestFixture]
    public class Test2 : Tests1TestBase
    {
        [Test]
        public async Task Sleep_3sec_1()
        {
            await Task.Delay(3000);
        }

        [Test]
        public async Task Sleep_3sec_2()
        {
            await Task.Delay(3000);
        }

        [Test]
        public async Task Sleep_3sec_3()
        {
            await Task.Delay(3000);
        }
    }
}
