using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace MyProject
{
    public class RawRequestResponseLogger : IEndpointBehavior, IDispatchMessageInspector
    {
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            Loggers.LoggerRaw.Trace("RawRequest: {0}", request);
            return null;
        }
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            Loggers.LoggerRaw.Trace("RawResponse: {0}", reply);
        }
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }
        public void Validate(ServiceEndpoint endpoint) { }
    }

    public class RawRequestResponseLoggerExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new RawRequestResponseLogger();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(RawRequestResponseLogger);
            }
        }
    }
}
