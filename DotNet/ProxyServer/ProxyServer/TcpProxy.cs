using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServer
{
    interface IProxy
    {
        Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp = null);
    }

    class TcpProxy : IProxy
    {
        public async Task Start(string remoteServerIp, ushort remoteServerPort, ushort localPort, string localIp)
        {
            //var clients = new ConcurrentDictionary<IPEndPoint, TcpClient>();

            //IPAddress localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.IPv6Any : IPAddress.Parse(localIp);
            //var server = new TcpListener(new IPEndPoint(localIpAddress, localPort));
            //server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            
            TcpListener server = new TcpListener(IPAddress.Any, localPort);
            server.Start();

            Console.WriteLine($"TCP proxy started {localPort} -> {remoteServerIp}|{remoteServerPort}");
            while (true)
            {
                try
                {
                    var ips = await Dns.GetHostAddressesAsync(remoteServerIp);

                    var remoteClient = await server.AcceptTcpClientAsync();
                    remoteClient.NoDelay = true;
                    var remoteClientEndpoint = (IPEndPoint)remoteClient.Client.RemoteEndPoint;

                    var remoteServerEndpoint = new IPEndPoint(ips.First(), remoteServerPort);

                    var clientTask = Task.Run(async () =>
                    {
                        bool establishedConnection = false;
                        try
                        {
                            using (var client = new TcpClient())
                            {
                                client.NoDelay = true;

                                try
                                {
                                    Console.WriteLine($"\nEstablishing {remoteClientEndpoint} => {remoteServerEndpoint}");
                                    await client.ConnectAsync(remoteServerEndpoint.Address, remoteServerEndpoint.Port);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine($"Failed to establish {remoteClientEndpoint} => {remoteServerEndpoint}");
                                    throw;
                                }

                                establishedConnection = true;
                                Console.WriteLine($"Established {remoteClientEndpoint} => {remoteServerEndpoint}");

                                var streamWithRemoteServer = client.GetStream();
                                var streamWithRemoteClient = remoteClient.GetStream();

                                var cts = new CancellationTokenSource();
                                var clientToServerTask = CopyStreamFromTo(streamWithRemoteClient, streamWithRemoteServer, cts.Token);
                                var serverToClientTask = CopyStreamFromTo(streamWithRemoteServer, streamWithRemoteClient, cts.Token);

                                await Task.WhenAny(clientToServerTask, serverToClientTask); // Client or server disconnected.

                                cts.Cancel();
                                await Task.WhenAll(clientToServerTask, serverToClientTask);

                                string clientSent = clientToServerTask.Result;
                                string serverSent = serverToClientTask.Result;

                                var clientBytes = Encoding.UTF8.GetBytes(clientSent);
                                var serverBytes = Encoding.UTF8.GetBytes(serverSent);
                                System.IO.File.WriteAllBytes("clientBytes", clientBytes);
                                System.IO.File.WriteAllBytes("serverBytes", serverBytes);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            if (establishedConnection)
                            {
                                Console.WriteLine($"Closed {remoteClientEndpoint} => {remoteServerEndpoint}");
                            }

                            remoteClient.Dispose();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }
            }
        }

        private async Task<string> CopyStreamFromTo(NetworkStream src, NetworkStream dest, CancellationToken ct)
        {
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes;
            do
            {
                try
                {
                    bytes = await src.ReadAsync(buffer, 0, buffer.Length, ct);
                }
                catch (OperationCanceledException)
                {
                    bytes = 0;
                }

                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                await dest.WriteAsync(buffer, 0, bytes);
            } while (bytes != 0);

            return messageData.ToString();
        }
    }
}