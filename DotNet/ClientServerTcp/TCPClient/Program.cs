using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace TCPClient
{
    class Program
    {
        const int PORT = 33333;

        static void Main(string[] args)
        {
            var ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

            Socket socket;

            while (true)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.ReceiveTimeout = 1000;

                    socket.Connect(ipPoint);

                    Console.WriteLine("Enter message");

                    string message = Console.ReadLine();
                    byte[] data = Encoding.Unicode.GetBytes(message);

                    socket.Send(data);

                    int bytesReceaved = 0;
                    data = new byte[256];
                    var builder = new StringBuilder();

                    do
                    {
                        bytesReceaved = socket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytesReceaved));
                    } while (socket.Available > 0);

                    Console.WriteLine("Servers answer: " + builder.ToString());                 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }

            Console.ReadLine();
        }
    }
}
