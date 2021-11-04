using System;
using System.Threading.Tasks;

namespace ServerApp
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine(System.IO.Directory.GetCurrentDirectory());

            string certFile = "./Certificates/server.pfx";
            string certPass = System.IO.File.ReadAllText("./Certificates/pass");

            await SslTcpServer.RunServer(certFile, certPass, args.Length == 0 ? 17777 : int.Parse(args[0]));

            return 0;
        }
    }
}
