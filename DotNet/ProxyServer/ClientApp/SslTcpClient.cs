﻿using System;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace ClientApp
{
    public class SslTcpClient
    {
        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return true;
        }
        public static async Task RunClient(string serverAddress, int serverPort)
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(serverAddress, serverPort);
            Console.WriteLine("Client connected.");

            bool useSsl = false;
            Stream stream;
            if (useSsl)
            {
                // Create an SSL stream that will close the client's stream.
                SslStream sslStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null
                    );
                // The server name must match the name on the server certificate.
                try
                {
                    sslStream.AuthenticateAsClient(serverAddress);
                }
                catch (AuthenticationException e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                    if (e.InnerException != null)
                    {
                        Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                    }
                    Console.WriteLine("Authentication failed - closing the connection.");
                    client.Close();
                    return;
                }

                stream = sslStream;
            }
            else
            {
                stream = client.GetStream();
            }


            int count = 0;
            while (count++ < 2)
            {
                // Encode a test message into a byte array.
                // Signal the end of the message using the "<EOF>".
                byte[] messsage = Encoding.UTF8.GetBytes("Hello from the client.<EOF>");
                // Send hello message to the server. 
                stream.Write(messsage);
                stream.Flush();

                // Read message from the server.
                string serverMessage = ReadMessage(stream);
                Console.WriteLine("Server says: {0}", serverMessage);

                await Task.Delay(1000);
            }

            // Close the client connection.
            client.Close();
            Console.WriteLine("Client closed.");
        }
        static string ReadMessage(Stream stream)
        {
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                bytes = stream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
    }
}