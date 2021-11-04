using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace DndBoard.SeleniumTestsBase.Fixtures
{
    public abstract class StartServerFixtureBase : IDisposable
    {
        private const string DotnetCmdName = "dotnet";
        private const int PortCheckTimoutMilliseconds = 500;
        private const int PortCheckWaitNextTryMilliseconds = 1000;
        private Process _process;

        protected abstract string ServerProjDir { get; }
        protected abstract int StartTimeoutSec { get; }
        protected abstract string Host { get; }
        protected abstract int Port { get; }


        public StartServerFixtureBase()
        {
            BuildServer();
            StartServer();
        }

        public void Dispose()
        {
            StopServer();
        }


        private void BuildServer()
        {
            Process buildProcess = new Process();
            StartProcess(buildProcess, "build . -c Release");
            buildProcess.Kill();
        }

        private void StartServer()
        {
            _process = new Process();
            StartProcess(_process, "run . -c Release");
            WaitUntilServerStarted();
        }

        private void StartProcess(Process buildProcess, string args)
        {
            buildProcess.StartInfo.WorkingDirectory = ServerProjDir;
            buildProcess.StartInfo.FileName = DotnetCmdName;
            buildProcess.StartInfo.UseShellExecute = false;
            buildProcess.StartInfo.Arguments = args;
            buildProcess.Start();
        }

        private void WaitUntilServerStarted()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!ServerStarted())
            {
                if (sw.ElapsedMilliseconds > StartTimeoutSec * 1000)
                    throw new TimeoutException($"Server failed to start in {StartTimeoutSec} seconds.");
                Thread.Sleep(PortCheckWaitNextTryMilliseconds);
            }
        }

        private bool ServerStarted() =>
            IsPortOpen(Host, Port, TimeSpan.FromMilliseconds(PortCheckTimoutMilliseconds));

        private bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using TcpClient client = new TcpClient();
                IAsyncResult result = client.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeout);
                client.EndConnect(result);
                return success;
            }
            catch
            {
                return false;
            }
        }

        private void StopServer()
        {
            _process.Kill();
        }
    }
}
