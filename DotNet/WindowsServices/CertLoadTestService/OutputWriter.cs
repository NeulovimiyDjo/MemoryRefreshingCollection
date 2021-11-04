using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace CertLoadTestService
{
    public static class OutputWriter
    {
        private static object _locker = new object();
        public static ILogger Logger { get; set; }

        private static void WriteToFile(string msg)
        {
            lock (_locker)
            {
                using StreamWriter writer = new StreamWriter("templog.txt", true);
                writer.WriteLine(msg);
                writer.Flush();
            }
        }

        private static void WriteToLog(string msg)
        {
            Logger.LogInformation(msg);
        }

        public static void Write(string msg)
        {
            string msgWithTime = $"{DateTimeOffset.Now}: {msg}";
            WriteToFile(msgWithTime);
            WriteToLog(msgWithTime);
        }
    }
}
