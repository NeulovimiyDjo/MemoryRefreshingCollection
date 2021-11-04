using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        Task Start(ushort localPort, string localIp = null);
    }

    class TcpProxy : IProxy
    {
        public async Task Start(ushort localPort, string localIp)
        {
            //var clients = new ConcurrentDictionary<IPEndPoint, TcpClient>();

            IPAddress localIpAddress = string.IsNullOrEmpty(localIp) ? IPAddress.IPv6Any : IPAddress.Parse(localIp);
            var server = new TcpListener(new IPEndPoint(localIpAddress, localPort));
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            server.Start();

            Console.WriteLine($"TCP proxy started at {localIpAddress}|{localPort}");
            while (true)
            {
                try
                {
                    //var ips = await Dns.GetHostAddressesAsync(remoteServerIp);

                    var remoteClient = await server.AcceptTcpClientAsync();
                    remoteClient.NoDelay = true;
                    var remoteClientEndpoint = (IPEndPoint)remoteClient.Client.RemoteEndPoint;


                    var streamWithRemoteClient = remoteClient.GetStream();
                    var cts = new CancellationTokenSource();
                    var remoteServerEndpoint = await GetServerEndpoint(streamWithRemoteClient, cts.Token);
                    await FinishSocks4Handshake(streamWithRemoteClient);

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
                                var clientToServerTask = CopyStreamFromTo(streamWithRemoteClient, streamWithRemoteServer, cts.Token, "clientBytes");
                                var serverToClientTask = CopyStreamFromTo(streamWithRemoteServer, streamWithRemoteClient, cts.Token, "serverBytes");

                                await Task.WhenAny(clientToServerTask, serverToClientTask); // Client or server disconnected.

                                cts.Cancel();
                                await Task.WhenAll(clientToServerTask, serverToClientTask);
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

        private async Task CopyStreamFromTo(NetworkStream src, NetworkStream dest, CancellationToken ct, string logFileName)
        {
            byte[] buffer = new byte[2048];
            int bytes;
            do
            {
                try
                {
                    bytes = await src.ReadAsync(buffer, 0, buffer.Length, ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception)
                {
                    return;
                }

                if (bytes > 0)
                {
                    using (var logFileStream = new FileStream(logFileName, FileMode.Append))
                    {
                        await logFileStream.WriteAsync(buffer.Take(bytes).ToArray());
                    }

                    int delay = StaticRandom.Instance.Next(20, 500);
                    await Task.Delay(delay);
                    await dest.WriteAsync(buffer, 0, bytes);
                }
            } while (bytes != 0);
        }

        private async Task<IPEndPoint> GetServerEndpoint(NetworkStream src, CancellationToken ct)
        {
            byte[] buffer = new byte[256];

            int bytes = await src.ReadAsync(buffer, 0, buffer.Length, ct);

            var port = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 2));
            var address = new IPAddress(buffer.Skip(4).Take(4).ToArray());

            return new IPEndPoint(address, port);
        }

        private async Task FinishSocks4Handshake(NetworkStream streamWithRemoteClient)
        {
            await streamWithRemoteClient.WriteAsync(new byte[]
                {
                    0x04, // reply version, null byte
                    0x5A, // reply code - Request granted
                    0xFF, 0xFF, // port - ignored
                    0xFF, 0xFF, 0xFF, 0xFF // address - ignored
                }
            );
        }
    }
}