using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProjectTests.Tests2
{
    [TestFixture]
    public class Test2 : Tests2TestBase
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
