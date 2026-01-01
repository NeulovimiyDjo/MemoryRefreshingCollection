using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalMutexAsyncSafeScopeProject
{
    // This is a wrapper over GlobalMutexScope
    // to dispose it on the same thread it was created
    // for example when used from async functions.
    public class GlobalMutexAsyncSafeScope : IDisposable
    {
        private ManualResetEvent _mutexReleaseEvent;
        private Exception _exception;

        private GlobalMutexAsyncSafeScope(string mutexId, int timeOut = -1)
        {
            _mutexReleaseEvent = new ManualResetEvent(false);
            var exitConstructorAllowedEvent = new ManualResetEvent(false);
            _exception = null;
            Task _ = Task.Run(() =>
            {
                GlobalMutexScope globalMutexScope = null;
                try
                {
                    try
                    {
                        globalMutexScope = new GlobalMutexScope(mutexId, timeOut);
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                        return;
                    }
                    finally
                    {
                        exitConstructorAllowedEvent.Set(); // Allow to exit from constructor
                    }
                    _mutexReleaseEvent.WaitOne(); // Block this fired'n'forgotten task until dispose is called
                }
                finally
                {
                    globalMutexScope?.Dispose(); // Dispose of GlobalMutexScope if it was successfully created
                }
            });
            exitConstructorAllowedEvent.WaitOne(); // Exit constructor only after GlobalMutexScope creation attempt
        }

        public static GlobalMutexAsyncSafeScope Create(string mutexId, int timeOut = -1)
        {
            var scope = new GlobalMutexAsyncSafeScope(mutexId, timeOut);
            if (scope._exception != null)
            {
                scope.Dispose();
                throw scope._exception;
            }
            return scope;
        }

        public void Dispose()
        {
            if (_mutexReleaseEvent != null)
            {
                _mutexReleaseEvent.Set();
                _mutexReleaseEvent = null;
            }
        }

        private class GlobalMutexScope : IDisposable
        {
            private readonly bool _hasHandle = false;
            private Mutex _mutex;
            private LockFileMutex _lockFilemutex;

            public GlobalMutexScope(string mutexId, int timeOut = -1)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                    catch
                    {
                        _mutex.Close();
                        _mutex = null;
                        throw;
                    }
                }
                else
                {
                    _lockFilemutex = new LockFileMutex(mutexId);
                    try
                    {
                        Stopwatch sw = new();
                        sw.Start();
                        while (!_lockFilemutex.TryAcquire())
                        {
                            if (timeOut >= 0 && sw.ElapsedMilliseconds > timeOut)
                                throw new TimeoutException($"Timeout waiting for exclusive access on mutex {mutexId}");
                            Thread.Sleep(50);
                        }
                    }
                    catch
                    {
                        _lockFilemutex.Dispose();
                        _mutex = null;
                        throw;
                    }
                }
            }

            public void Dispose()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (_mutex != null)
                    {
                        if (_hasHandle)
                            _mutex.ReleaseMutex();
                        _mutex.Close();
                        _mutex = null;
                    }
                }
                else
                {
                    if (_lockFilemutex != null)
                    {
                        _lockFilemutex.Dispose();
                        _lockFilemutex = null;
                    }
                }
            }
        }

        private class LockFileMutex : IDisposable
        {
            private readonly string _filePath;
            private FileStream _fileStream;

            public LockFileMutex(string fileName)
            {
                string mutexesDir = "/tmp/.lfmutexes";
                string filePath = Path.GetFullPath(Path.Combine(mutexesDir, fileName));
                _filePath = filePath;
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            }

            public bool TryAcquire()
            {
                try
                {
                    _fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    return true;
                }
                catch (IOException ex) when (ex.Message.Contains(_filePath))
                {
                    return false;
                }
            }

            public void Dispose()
            {
                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                    try
                    {
                        File.Delete(_filePath);
                    }
                    catch
                    {
                    }
                }
                GC.SuppressFinalize(this);
            }
        }
    }
}
