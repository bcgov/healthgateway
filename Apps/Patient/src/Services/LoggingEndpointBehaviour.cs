namespace HealthGateway.Service.Patient
{
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Channels;
    using System;

    ///   
    public class LoggingEndpointBehaviour : IEndpointBehavior
    {
        ///   
        private IClientMessageInspector messageInspector;

        ///
        public LoggingEndpointBehaviour(IClientMessageInspector messageInspector)
        {
            this.messageInspector = messageInspector ?? throw new ArgumentNullException(nameof(messageInspector));
        }

        ///
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        ///
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(this.messageInspector);
        }

        ///
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        ///
        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}