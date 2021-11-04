public static async Task<bool> TestSomeService()
	{
		var binding = new BasicHttpBinding();
		binding.Security.Mode = BasicHttpSecurityMode.Transport; // Required for ssl
		binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

		binding.MaxBufferSize = int.MaxValue;
		binding.MaxReceivedMessageSize = int.MaxValue;
		binding.MaxBufferPoolSize = int.MaxValue;
		binding.TransferMode = TransferMode.Streamed;

		using var client = new SomeServiceClient(binding, new EndpointAddress("https://localhost/some_subfolder/WCFStreamFilesExampleService/someservice.svc"));

		if (true) // if dev
		{
			client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = // For development only.
				new X509ServiceCertificateAuthentication()
				{
					CertificateValidationMode = X509CertificateValidationMode.None,
					RevocationMode = X509RevocationMode.NoCheck
				};
		}

		client.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(300);
		
		client.ClientCredentials.ClientCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, "f94adb43c29b78f9a81d7db7085c97a6a355a619");


		using var stream = System.IO.File.OpenRead("c:/some_very_big_file");
		var response = await await Helpers.GetResponseAsync(client, () => client.TestMethod2Async("somefileName", stream));
		return response.UploadSucceeded;
	}
}