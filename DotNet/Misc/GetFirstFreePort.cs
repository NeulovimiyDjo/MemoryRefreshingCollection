public static int GetAvailablePort(int startingPort)
{
    if (startingPort > ushort.MaxValue) throw new ArgumentException($"Can't be greater than {ushort.MaxValue}", nameof(startingPort));
    var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

    var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
    var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
    var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
    var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                                         .Concat(udpListenersEndpoints)
                                         .Select(e => e.Port);

    return Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).FirstOrDefault();
}

private static string GetFreePort()
{
    // From https://stackoverflow.com/a/150974/4190785
    var tcpListener = new TcpListener(IPAddress.Loopback, 0);
    tcpListener.Start();
    var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
    tcpListener.Stop();
    return port.ToString();
}
