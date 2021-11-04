using System;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalMutexAsyncSafeScopeProj
{
    public class GlobalMutexAsyncSafeScope : IDisposable
    {
        private ManualResetEvent _mutexReleaseEvent;


        public GlobalMutexAsyncSafeScope(string mutexId, int timeOut = -1)
        {
            if (_mutexReleaseEvent != null)
                throw new Exception("Mutex already entered");
            _mutexReleaseEvent = new ManualResetEvent(false);
            var mutexEnteredEvent = new ManualResetEvent(false);
            Task _ = Task.Run(() =>
            {
                using var _ = new GlobalMutexScope(mutexId, timeOut);
                mutexEnteredEvent.Set();
                _mutexReleaseEvent.WaitOne();
            }).ContinueWith(t => HandleException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            mutexEnteredEvent.WaitOne();
        }

        public void Dispose()
        {
            if (_mutexReleaseEvent != null)
            {
                _mutexReleaseEvent.Set();
                _mutexReleaseEvent = null;
            }
        }


        private static void HandleException(AggregateException exception)
        {
            foreach (var ex in exception.Flatten().InnerExceptions)
                throw new Exception(ex.Message);
        }

        private class GlobalMutexScope : IDisposable
        {
            public bool _hasHandle = false;
            private Mutex _mutex;

            public GlobalMutexScope(string mutexId, int timeOut = -1)
            {
                _mutex = new Mutex(false, mutexId);
                try
                {
                    if (timeOut < 0)
                        _hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
                    else
                        _hasHandle = _mutex.WaitOne(timeOut, false);

                    if (_hasHandle == false)
                        throw new TimeoutException($"Timeout waiting for exclusive access on mutex {mutexId}");
                }
                catch (AbandonedMutexException)
                {
                    _hasHandle = true;
                }
            }

            public void Dispose()
            {
                if (_mutex != null)
                {
                    if (_hasHandle)
                        _mutex.ReleaseMutex();
                    _mutex.Close();
                }
            }
        }
    }
}
