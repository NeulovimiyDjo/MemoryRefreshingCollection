using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                string msg = $"Now is {DateTime.Now:dd/MM/yyyy hh:mm:ss}";
                WriteToFile(msg);
                await Task.Delay(1000, stoppingToken);
            }
        }

        object _locker = new object();
        private void WriteToFile(string msg)
        {
            lock (_locker)
            {
                using (StreamWriter writer = new StreamWriter("templog.txt", true))
                {
                    writer.WriteLine(msg);
                    writer.Flush();
                }
            }
        }
    }
}
