using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace MyProject
{
    public static class HelperClient
    {
        public static ClientType GetClientImpl<ClientType, ClientInterface>(
            string address, IntegrationModule module
        ) where ClientType : ClientBase<ClientInterface> where ClientInterface : class
        {
            var endpointAddress = new EndpointAddress(address);

            var binding = new BasicHttpBinding();
            if (endpointAddress.Uri.Scheme != "http")
                binding.Security.Mode = BasicHttpSecurityMode.Transport;


            binding.MaxBufferSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferPoolSize = int.MaxValue;
            if (_sendTimeoutSeconds.HasValue)
                binding.SendTimeout = TimeSpan.FromSeconds(_sendTimeoutSeconds.Value);

            var client = (ClientType)Activator.CreateInstance(typeof(ClientType), binding, new EndpointAddress(address));

            if (endpointAddress.Uri.Scheme != "http")
                client.Endpoint.EndpointBehaviors.Add(new TlsVersionEnforceEndpointBehavior());
            client.Endpoint.EndpointBehaviors.Add(new RawRequestResponseLogger());

            if (ignoreServerCertificate)
            {
                client.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = // For development only.
                    new X509ServiceCertificateAuthentication()
                    {
                        CertificateValidationMode = X509CertificateValidationMode.None,
                        RevocationMode = X509RevocationMode.NoCheck
                    };
            }

            if (UseCertAuth(module))
                SetCertificate(client);

            return client;
        }


        // To restrict incoming requests to TLSv1.2+ use IIS binding settings (disable legacy tls).
        // This settings is only available on Windows server 2019+. Otherwise configure Schannel in OS registry.
        public class TlsVersionEnforceEndpointBehavior : IEndpointBehavior
        {
            public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12;
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

        public class RawRequestResponseLogger : IEndpointBehavior, IClientMessageInspector
        {
            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.ClientMessageInspectors.Add(this);
            }
            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                SapPOLoggers.LoggerOutRaw.Trace("RawRequest: {0}", request);
                return null;
            }
            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                SapPOLoggers.LoggerOutRaw.Trace("RawResponse: {0}", reply);
            }
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
            public void Validate(ServiceEndpoint endpoint) { }
        }


        public static async Task<ResponseType> ExecuteWithHeaderAsyncImpl<ResponseType, ClientInterface>(
            ClientBase<ClientInterface> client,
            Func<Task<ResponseType>> getResponseFunc,
            IntegrationModule module
        ) where ClientInterface : class
        {
			Task<ResponseType> response;
			using (new HeaderCreator<ClientInterface>(client, module))
			{
				response = getResponseFunc();
			} // Dispose before awaiting async client method call (because awaiting may switch thread).

			return await response;
        }
    }
}


private static readonly string certificateThumbprint = Regex.Replace(
            "thumb", @"[^\da-fA-F]",
            string.Empty
        ).ToUpper();

private static readonly StoreLocation certificateStoreLocation = (StoreLocation)2;
private static readonly StoreName certificateStoreName = (StoreName)5;


public static void SetCertificate<ClientInterface>(ClientBase<ClientInterface> client) where ClientInterface : class
{
	(client.Endpoint.Binding as BasicHttpBinding).Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
	client.ClientCredentials.ClientCertificate.SetCertificate(certificateStoreLocation, certificateStoreName, X509FindType.FindByThumbprint, certificateThumbprint);
}


public class HeaderCreator<ClientInterface> : IDisposable where ClientInterface : class
{
	private OperationContextScope scope;

	public HeaderCreator(ClientBase<ClientInterface> client, IntegrationModule module)
	{
		scope = new OperationContextScope(client.InnerChannel);

		try
		{
			HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
			requestMessage.Headers["Header1"] = "Header1Value";
			if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
			{
				OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
			}
			else
			{
				OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, requestMessage);
			}
		}
		catch (Exception)
		{
			scope.Dispose();
			throw;
		}
	}

	public void Dispose()
	{
		scope.Dispose();
	}
}
