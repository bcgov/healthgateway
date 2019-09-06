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
namespace HealthGateway.Service.Patient
{
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.ServiceModel.Channels;
    using System;

    /// <summary>
    /// Implementation of EndpoingBehaviour for logging purposes.
    /// </summary>
    public class LoggingEndpointBehaviour : IEndpointBehavior
    {
        private IClientMessageInspector messageInspector;

        /// <summary>
        /// Constructor that requires am IClientMessageInspector
        /// </summary>
        public LoggingEndpointBehaviour(IClientMessageInspector messageInspector)
        {
            this.messageInspector = messageInspector ?? throw new ArgumentNullException(nameof(messageInspector));
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour
        /// </summary>
        /// <param name="id">The patient id.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour
        /// </summary>
        /// <param name="id">The patient id.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(this.messageInspector);
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour
        /// </summary>
        /// <param name="id">The patient id.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implementation of IEndpointBehaviour
        /// </summary>
        /// <param name="id">The patient id.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}