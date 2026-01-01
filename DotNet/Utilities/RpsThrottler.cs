using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RpsThrottlerProject;

public class RpsThrottler
{
    private readonly int _delayMs;
    private readonly Stopwatch _sw = new();
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public RpsThrottler(int delayMs)
    {
        _delayMs = delayMs;
    }

    public async Task<TResponse> Do<TResponse>(Func<Task<TResponse>> makeRequestFunc)
    {
        await WaitRemainingDelaySinceLastRequestIfNecessary();
        return await makeRequestFunc();
    }

    private async Task WaitRemainingDelaySinceLastRequestIfNecessary()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (_sw.IsRunning)
            {
                int timeToDelay = (int)Math.Max(_delayMs - _sw.ElapsedMilliseconds, 0);
                if (timeToDelay > 0)
                    await Task.Delay(timeToDelay);
            }
            _sw.Restart();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
