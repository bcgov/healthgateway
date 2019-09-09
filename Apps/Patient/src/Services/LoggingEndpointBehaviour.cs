//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.PatientService
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Implementation of EndpoingBehaviour for logging purposes.
    /// </summary>
    public class LoggingEndpointBehaviour : IEndpointBehavior
    {
        private IClientMessageInspector messageInspector;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingEndpointBehaviour"/> class.
        /// Constructor that requires am IClientMessageInspector.
        /// </summary>
        /// <param name="messageInspector">The client message inspector.</param>
        public LoggingEndpointBehaviour(IClientMessageInspector messageInspector)
        {
            this.messageInspector = messageInspector ?? throw new ArgumentNullException(nameof(messageInspector));
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour.
        /// </summary>
        /// <param name="endpoint">The service endpoint.</param>
        /// <param name="bindingParameters">The binding parameter collection.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour.
        /// </summary>
        /// <param name="endpoint">The endpoint service.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (clientRuntime is null)
            {
                throw new ArgumentNullException(nameof(clientRuntime));
            }

            clientRuntime.ClientMessageInspectors.Add(this.messageInspector);
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour.
        /// </summary>
        /// <param name="endpoint">The endpoint service.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour.
        /// </summary>
        /// <param name="endpoint">The service endpoint.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}