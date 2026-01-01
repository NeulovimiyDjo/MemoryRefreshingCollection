using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MyProjectTests;

[assembly: LevelOfParallelism(9999)]
[SetUpFixture]
[Parallelizable(ParallelScope.Self | ParallelScope.Fixtures)]
[Order(1)]
public sealed class MySetupFixture : ManuallyParallelizableSetupFixtureBase { }
public sealed class MyTest : SemaphoreTestBase { }
public abstract class ManuallyParallelizableSetupFixtureBase
{
    private const int MaxParallelNsExecuting = 4;
    private static readonly SemaphoreSlim _maxParallelNsExecutingSemaphore = new(MaxParallelNsExecuting, MaxParallelNsExecuting);
    private static readonly ConcurrentDictionary<string, int> _executingQueue = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            int thisFixtureOrder = this.GetType().GetCustomAttribute<OrderAttribute>()?.Order ?? 0;
            _executingQueue.TryAdd(this.GetType().FullName, thisFixtureOrder);
            await Task.Delay(1000);

            Func<int> getQueuedFixturesWithLesserOrderCount = () => _executingQueue.Where(x => x.Value < thisFixtureOrder).Count();
            while (getQueuedFixturesWithLesserOrderCount() >= MaxParallelNsExecuting)
                await Task.Delay(100);

            await _maxParallelNsExecutingSemaphore.WaitAsync();
            await SetUpCore();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Unexpected exception during {nameof(OneTimeSetUp)}:" +
                $"\n-Type: {ex.GetType()}" +
                $"\n-Message: {ex.Message}" +
                $"\n-StackTrace:" +
                $"\n{ex.StackTrace}"
            );
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        try
        {
            await TearDownCore();
        }
        finally
        {
            _executingQueue.TryRemove(this.GetType().FullName, out int _);
            _maxParallelNsExecutingSemaphore.Release();
        }
    }
}
