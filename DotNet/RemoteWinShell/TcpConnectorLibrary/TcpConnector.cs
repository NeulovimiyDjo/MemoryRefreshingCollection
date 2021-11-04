using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TcpConnectorLibrary
{
    public class TcpConnector
    {
        const int PORT = 33333;

        public static Socket StartServer()
        {
            var ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

            var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(ipPoint);

                listenSocket.Listen(10);

                Console.WriteLine("Server started. Waiting for connections...");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return listenSocket;
        }

        public static string ReadMessage(Socket listenSocket, out Socket handler)
        {
            var builder = new StringBuilder();

            handler = null;
            try
            {
                handler = listenSocket.Accept();
                int bytesReceaved = 0;
                byte[] data = new byte[256];


                do
                {
                    bytesReceaved = handler.Receive(data);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytesReceaved));
                } while (handler.Available > 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return builder.ToString();
        }

        public static void ReadFile(Socket listenSocket, out Socket handler, string fileName)
        {
            handler = null;
            try
            {
                handler = listenSocket.Accept();
                int bytesReceaved = 0;
                byte[] data = new byte[256];

                using (FileStream fs = File.Create(fileName))
                {
                    // do nothing
                }

                do
                {
                    bytesReceaved = handler.Receive(data);
                    AppendToFile(fileName, data, bytesReceaved);
                } while (handler.Available > 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void AppendToFile(string fileName, byte[] data, int bytesReceaved)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write))
            {
                fs.Write(data, 0, bytesReceaved);
            }
        }

        public static void SendMessage(string message, Socket handler)
        {
            // idiot winexe uses 866 encoding although Process.StandartOutput.CurrentEncoding == 1251
            // so 866-encoded message is decoded by c# as 1251
            // so we need to encode it back to bytes the same way it was decoded (1251)          
            byte[] msg1251 = Encoding.GetEncoding(1251).GetBytes(message);
            // and decode it the right way (866) back to string
            message = Encoding.GetEncoding(866).GetString(msg1251);

            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                handler.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void CloseConnection(Socket handler)
        {
            try
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}