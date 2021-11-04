using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string address = Dns.GetHostAddresses("serverapp")[0].ToString();
            ushort port = 17777;
            Console.WriteLine("Proxy connects to: " + address + ":" + port.ToString());

            try
			{
                var proxy = new TcpProxy();
                var t = proxy.Start(address, port, 18888, "127.0.0.1");

                Task.WhenAll(t).Wait();
			}
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured : {ex}");
            }
        }
    }
}
