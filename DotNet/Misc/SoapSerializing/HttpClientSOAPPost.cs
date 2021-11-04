		private static IConfigurationRoot _configuration = new ConfigurationBuilder()
			.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
			.AddJsonFile(
				 path: "Settings.json",
				 optional: false,
				 reloadOnChange: true)
		   .Build();
		
		private readonly HttpClient client = new HttpClient();

        public async Task<(HttpStatusCode, string)> SendSOAPRequest(string soapXmlBody, string endpointName)
        {
            using StringContent requestContent = new(soapXmlBody, Encoding.UTF8, "text/xml");
            string serviceAddress = string.Format(
                _configuration.GetValue<string>("SomeAddress"),
                endpointName
            );
            using HttpRequestMessage request = new(HttpMethod.Post, serviceAddress);
            request.Headers.Add("SOAPAction", "http://somesrvns/ISomeService/SomeOperation");
            request.Content = requestContent;
            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            string responseContent = await response.Content.ReadAsStringAsync();
            return (response.StatusCode, responseContent);
        }