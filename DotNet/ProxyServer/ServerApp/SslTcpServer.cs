using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace ServerApp
{
    public sealed class SslTcpServer
    {
        static X509Certificate serverCertificate = null;
        // The certificate parameter specifies the name of the file 
        // containing the machine certificate.
        public static async Task RunServer(string certFile, string certPass, int port)
        {
            serverCertificate = new X509Certificate(certFile, certPass);
            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server started.");

            int clientCount = 0;
            while (true)
            {
                try
                {
                    // Application blocks while waiting for an incoming connection.
                    // Type CNTL-C to terminate the server.
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    var clientTask = Task.Run(async () =>
                    {
                        int clientNum = ++clientCount;
                        try
                        {
                            Console.WriteLine($"\nClient {clientNum} connected.");
                            await ProcessClient(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            Console.WriteLine($"Client {clientNum} disconnected.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        static async Task ProcessClient(TcpClient client)
        {
            bool useSsl = false;
            Stream stream;
            if (useSsl)
            {
                // A client has connected. Create the 
                // SslStream using the client's network stream.
                SslStream sslStream = new SslStream(
                    client.GetStream(), false);

                stream = sslStream;
            }
            else
            {
                stream = client.GetStream();
            }
            
            // Authenticate the server but don't require the client to authenticate.
            try
            {
                if (useSsl)
                {
                    await ((SslStream)stream).AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                    // Display the properties and settings for the authenticated stream.
                    //DisplaySecurityLevel(sslStream);
                    //DisplaySecurityServices(sslStream);
                    //DisplayCertificateInformation(sslStream);
                    //DisplayStreamProperties(sslStream);
                }


                // Set timeouts for the read and write to 5 seconds.
                stream.ReadTimeout = 5000;
                stream.WriteTimeout = 5000;

                while (true)
                {
                    // Read a message from the client.   
                    Console.WriteLine("Waiting for client message...");
                    string messageData = await ReadMessage(stream);
                    if (messageData.Length == 0) // End of stream reached.
                    {
                        break;
                    }
                    Console.WriteLine("Received: {0}", messageData);


                    // Write a message to the client.
                    byte[] message = Encoding.UTF8.GetBytes("Hello from the server.<EOF>");
                    Console.WriteLine("Sending hello message.");
                    await stream.WriteAsync(message);
                }
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                stream.Close();
                client.Close();
            }
        }
        static async Task<string> ReadMessage(Stream stream)
        {
            // Read the  message sent by the client.
            // The client signals the end of the message using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                // Read the client's test message.
                bytes = await stream.ReadAsync(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                // Check for EOF or an empty message.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
    }
}