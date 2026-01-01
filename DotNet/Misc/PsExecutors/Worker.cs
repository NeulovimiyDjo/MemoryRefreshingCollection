using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PsExecutors
{
    public class Worker : BackgroundService
    {
        private const string UtilsDir = "../../../../Utils";
        private const string Script1Path = UtilsDir + "/Script1.ps1";

        private readonly FileSystemWatcher _watcher = new();
        private readonly PsExecutor _psExecutor = new();
        private readonly ILogger _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _watcher.Path = SupplyDir;
            _watcher.Filter = "*.zip";
            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.IncludeSubdirectories = false;
            _watcher.Created += OnFileCreated;
            _watcher.EnableRaisingEvents = true;

            stoppingToken.Register(OnServiceStop);
            return Task.CompletedTask;
        }

        private void OnServiceStop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                Dictionary<string, object> arguments = new();
                arguments.Add("Param1", Path.GetFullPath(e.FullPath));
                string res = _psExecutor.RunPs1File(Path.GetFullPath(Script1Path), arguments);
                OutputWriter.WriteToFile(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
