using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPServer
{
    class Program
    {
        const int PORT = 33333;

        static void Main(string[] args)
        {
            var ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

            var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(ipPoint);

                listenSocket.Listen(10);

                Console.WriteLine("Server started. Waiting for connections...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();

                    var builder = new StringBuilder();
                    int bytesReceaved = 0;
                    byte[] data = new byte[256];


                    do
                    {
                        bytesReceaved = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytesReceaved));
                    } while (handler.Available > 0);


                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    string message = "Your message is sent";
                    data = Encoding.Unicode.GetBytes(message);
                    handler.Send(data);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close(); 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
