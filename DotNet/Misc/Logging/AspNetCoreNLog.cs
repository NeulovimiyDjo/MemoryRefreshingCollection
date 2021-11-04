private static IHost CreateWebServiceHost(string serviceDllPath, int port)
{
	string serviceDir = Path.GetDirectoryName(serviceDllPath);
	Assembly a = Assembly.LoadFrom(serviceDllPath);
	Type startupType = a.GetTypes().Where(x => x.Name == "Startup").First();
	IHost host = CreateHostBuilder(serviceDir, startupType, port).Build();
	return host;
}

private static IHostBuilder CreateHostBuilder(string serviceDir, Type startupType, int port) =>
	Host.CreateDefaultBuilder()
	.ConfigureWebHostDefaults(webHostBuilder =>
	{
		webHostBuilder
			.UseUrls($"http://localhost:{port}/")
			.UseStartup(startupType)
			.UseContentRoot(serviceDir);
	})
	.ConfigureHostConfiguration(c => c.SetBasePath(serviceDir))
	.ConfigureLogging(logging =>
	{
		logging.ClearProviders();
		logging.SetMinimumLevel(LogLevel.Trace);
	})
	.UseNLog();