using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        private Thread _workerThread;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _workerThread = new Thread(new ThreadStart(RunLogger));
            _workerThread.Start();
        }

        private void RunLogger()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            while (true)
            {
                string msg = $"Now is {DateTime.Now:dd/MM/yyyy hh:mm:ss}";
                WriteToFile(msg);
                Thread.Sleep(1000);
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

        protected override void OnStop()
        {
            _workerThread.Abort();
        }
    }
}
