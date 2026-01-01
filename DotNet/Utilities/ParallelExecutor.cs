using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelExecutorProject
{
    public static class ParallelExecutor
    {
        public static async Task ProcessItemsConcurrently<TItem>(
            Func<TItem, Task> doWorkFunc,
            IEnumerable<TItem> itemCollection,
            int maxThreads,
            CancellationToken ct)
        {
            if (maxThreads <= 1)
            {
                foreach (TItem item in itemCollection)
                {
                    ct.ThrowIfCancellationRequested();
                    await doWorkFunc(item);
                }
            }
            else
            {
                int count = 0;
                List<Task> doWorkTasks = new();
                List<Exception> exceptions = new();
                foreach (TItem item in itemCollection)
                {
                    if (doWorkTasks.Any(t => t.Exception is not null))
                        break;
                    if (ct.IsCancellationRequested)
                        break;

                    doWorkTasks.Add(DowWork(item, count++));
                    if (doWorkTasks.Count == maxThreads)
                    {
                        await Task.WhenAny(doWorkTasks.ToArray());
                        doWorkTasks.RemoveAll(t => t.IsCompleted && t.Exception is null);
                    }
                }

                await Task.WhenAll(doWorkTasks.ToArray()).ContinueWith(t =>
                {
                    if (t.Exception is not null)
                        exceptions.AddRange(t.Exception.InnerExceptions);
                });

                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions.ToArray());
                ct.ThrowIfCancellationRequested();

                async Task DowWork(TItem item, int count)
                {
                    if (count < maxThreads)
                        await Task.Delay(100 * count);

                    await doWorkFunc(item);
                }
            }
        }
    }
}
