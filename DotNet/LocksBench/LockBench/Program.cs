using System;
using System.Diagnostics;
using System.Threading;

namespace LockBench
{
  class Program
  {
    static void Main(string[] args)
    {
      Object _lock = new Object();
      var rw = new ReaderWriterLockSlim();

      int count = 2;

      var w = new Stopwatch();
      w.Start();
      for (int i = 0; i < 10000000; i++)
      {
        Monitor.Enter(_lock);
        try
        {
          count = count * count % 1023;
        }
        finally
        {
          Monitor.Exit(_lock);
        }

        //rw.EnterReadLock();
        //try
        //{
        //  count = count * count % 1023;
        //}
        //finally
        //{
        //  rw.ExitReadLock();
        //}

        //lock(_lock)
        //{
        //  count = count * count % 1023;
        //}

        //Interlocked.Exchange(ref count, count * count % 1023);

        //count = count * count % 1023;
      }
      w.Stop();
      Console.WriteLine(count + " Time(ms): " + w.ElapsedMilliseconds);

      Console.ReadLine();
    }
  }
}
