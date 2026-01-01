using System;
using System.IO;

namespace WcfHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string serviceDllPath = Path.GetFullPath(args[0]);
            string serviceConfigPath = Path.GetFullPath(args[1]);
            RemoteProxy.CreateAndStartAllServices(serviceDllPath, serviceConfigPath);
            Console.ReadLine();
        }
    }
}
