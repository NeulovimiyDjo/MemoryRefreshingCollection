using System;
using System.Net;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {     
        public static async Task<int> Main(string[] args)
        {
            string address = Dns.GetHostAddresses(
                args.Length < 1 ? "serverapp" : args[0]
            )[0].ToString();
            int port = args.Length < 2 ? 17777 : int.Parse(args[1]);
            Console.WriteLine("Client connects to: " + address + ":" + port.ToString());

            while (true)
            {
                try
                {
                    await SslTcpClient.RunClient(address, port);

                    await Task.Delay(10000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return 0;
        }
    }
}
