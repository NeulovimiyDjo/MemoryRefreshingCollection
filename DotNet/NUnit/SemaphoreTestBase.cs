using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyProjectTests;

public abstract class SemaphoreTestBase
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _nsSemaphores = new();
    private static readonly object _nsSemaphoresInitializationLock = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // The suggested answer in https://github.com/nunit/nunit/issues/4267 runs this in parallel in the same namespace.
        lock (_nsSemaphoresInitializationLock)
        {
            if (!_nsSemaphores.ContainsKey(this.GetType().Namespace))
                _nsSemaphores[this.GetType().Namespace] = new(1, 1);
        }
        await _nsSemaphores[this.GetType().Namespace].WaitAsync();

        await SetUpCore();
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
            _nsSemaphores[this.GetType().Namespace].Release();
        }
    }
}
