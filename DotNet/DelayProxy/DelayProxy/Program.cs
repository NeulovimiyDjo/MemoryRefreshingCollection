using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
                var proxy = new TcpProxy();
                var t = proxy.Start(18888, "127.0.0.1");

                Task.WhenAll(t).Wait();
			}
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured : {ex}");
            }
        }
    }
}
