using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace AvailabilityCheckerProject
{
    public static class AvailabilityChecker
    {
        public void WaitUntilTcpPortAvailable(string host, int port, int startTimeoutSec)
        {
            Stopwatch sw = new();
            sw.Start();
            while (!IsPortOpen())
            {
                if (sw.ElapsedMilliseconds > startTimeoutSec * 1000)
                    throw new TimeoutException($"Server failed to start in {startTimeoutSec} seconds.");
                Thread.Sleep(1000);
            }

            bool IsPortOpen()
            {
                try
                {
                    using TcpClient client = new TcpClient();
                    IAsyncResult result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(1000));
                    client.EndConnect(result);
                    return success;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static async Task WaitUntilDatabaseAvailable(DbConnection connection, int timeoutSeconds)
        {
            DateTime start = DateTime.UtcNow;
            bool connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(timeoutSeconds) > DateTime.UtcNow)
            {
                try
                {
                    await connection.OpenAsync();
                    connectionEstablised = true;
                }
                catch
                {
                    await Task.Delay(500);
                }
            }

            if (!connectionEstablised)
                throw new Exception($"Connection to database could not be established within {timeoutSeconds} seconds.");
        }
    }
}
