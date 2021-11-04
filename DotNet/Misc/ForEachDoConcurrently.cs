
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public static class SomeClass
{
    private static readonly int _maxThreads = 3;
    private static readonly int _avgOpTime = 500;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();

    public static async Task Run()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        int[] items = new int[] { 11, 22, 33, 44, 55, 66, 77 };
        await ForEachDoConcurrently(items, async (item, num) =>
        {
            if (num < _maxThreads)
                await Task.Delay(_avgOpTime * num / _maxThreads);
            await DoWork(item);
        }, _maxThreads, cancellationToken);
    }

    public static async Task ForEachDoConcurrently<T>(
        IEnumerable<T> items, Func<T, int, Task> action, int maxConcurrentTasks, CancellationToken cancellationToken = default)
    {
        int num = 0;
        List<Task> tasks = new();
        foreach (T item in items)
        {
            if (tasks.Any(t => t.Exception is not null))
                break;
            if (cancellationToken.IsCancellationRequested)
                break;

            tasks.Add(action(item, num++));
            if (tasks.Count == maxConcurrentTasks)
            {
                await Task.WhenAny(tasks);
                tasks.RemoveAll(t => t.IsCompleted && t.Exception is null);
            }
        }

        await Task.WhenAll(tasks).ContinueWith(t =>
        {
            if (t.Exception is not null)
                throw t.Exception;
        });
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static async Task DoWork(int item)
    {
        await Task.Delay(_avgOpTime);
        Console.WriteLine(item);
        if (item > 220)
            throw new Exception($"ex: {item}");
        if (item > 440)
            _cancellationTokenSource.Cancel();
    }
}
