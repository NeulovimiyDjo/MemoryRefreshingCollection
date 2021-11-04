var handler = new HttpClientHandler()
{
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
};

var client = new HttpClient(handler);






client.Endpoint.EndpointBehaviors.Add(new TlsVersionEnforceEndpointBehavior());

public class TlsVersionEnforceEndpointBehavior : IEndpointBehavior
{
	public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls11;
	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
	{
		bindingParameters.Add(new Func<HttpClientHandler, HttpMessageHandler>(x =>
		{
			x.SslProtocols = this.SslProtocols;
			return x; // You can just return the modified HttpClientHandler
		}));
	}
	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }
	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
	public void Validate(ServiceEndpoint endpoint) { }
}











public bool Validate(X509Certificate2 cert, out string error)
{
	error = "";


	X509Chain ch = new X509Chain();
	ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
	bool built = ch.Build(cert);

	if (!built)
	{
		var elements = new List<X509ChainElement>();
		foreach (X509ChainElement elem in ch.ChainElements)
			elements.Add(elem);

		error = string.Join(
			"\n",
			elements.Select(x => ChainElementToString(x))
		);

		return false;
	}

	return false;
}


private string ChainElementToString(X509ChainElement elem)
	=> string.Join(
		"\n",
		"Subject: " + elem.Certificate.Subject,
		"-IsValid: " + elem.Certificate.Verify(),
		"-ChainStatusInfo: " + ChainStatusToString(elem.ChainElementStatus)
	);

private string ChainStatusToString(X509ChainStatus[] statuses)
	=> string.Join(
		"\n",
		statuses.Select(x => x.StatusInformation.Trim())
	);