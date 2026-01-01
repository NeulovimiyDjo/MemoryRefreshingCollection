using System.Runtime.InteropServices;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new System.Exception("os is not linux");
            }
        }

        [Fact]
        public void Test2()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new System.Exception("os is not windows");
            }
        }
    }
}