using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GlobalMutexAsyncSafeScopeTests
{
    [TestFixture]
    public class GlobalMutexAsyncSafeScopeTests
    {
        private const string TestMutexName = "TestMutexName";
        private readonly Stopwatch _sw = new();

        [SetUp]
        public void SetUp()
        {
            _sw.Restart();
        }

        [Test]
        public async Task JobsInScopeDontRunParallel()
        {
            var t1 = Task.Run(async () =>
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName);
                await Task.Delay(1000);
            });

            var t2 = Task.Run(async () =>
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName);
                await Task.Delay(1000);
            });

            await Task.WhenAll(t1, t2);
            AssertTime(1950, 2700);
        }

        [Test]
        public void NoDeadlockWhenTwoScopesCreatedInSameThreadWithTimeOut()
        {
            using var _1 = GlobalMutexAsyncSafeScope.Create(TestMutexName);
            try
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName, 100);
            }
            catch
            {
            }

            AssertTime(50, 800);
        }

        [Test]
        public async Task ScopeCreationBlocksUntilReleaseFromOtherThread()
        {
            var t = Task.Run(async () =>
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName);
                await Task.Delay(500);
            });

            await Task.Delay(100);
            using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName, 1500);
            await Task.WhenAll(t);

            AssertTime(450, 1300);
        }

        [Test]
        public async Task ScopeCreationThrowsExceptionOnTimeOut()
        {
            var t = Task.Run(async () =>
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName);
                await Task.Delay(500);
            });

            await Task.Delay(100);
            var ex = Assert.Throws<TimeoutException>(() =>
            {
                using var _ = GlobalMutexAsyncSafeScope.Create(TestMutexName, 100);
            });
            await Task.WhenAll(t);

            Assert.AreEqual($"Timeout waiting for exclusive access on mutex {TestMutexName}", ex.Message);
        }

        private void AssertTime(int min, int max)
        {
            _sw.Stop();
            Assert.True(_sw.ElapsedMilliseconds >= min, $"Expected min time {min} but was {_sw.ElapsedMilliseconds}");
            Assert.True(_sw.ElapsedMilliseconds <= max, $"Expected max time {max} but was {_sw.ElapsedMilliseconds}");
        }
    }
}
