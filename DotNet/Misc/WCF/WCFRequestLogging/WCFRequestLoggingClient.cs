
client.Endpoint.EndpointBehaviors.Add(new RawRequestResponseLogger());

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