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
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of IClientMessageInspector for loging purposes.
    /// </summary>
    public class LoggingMessageInspector : IClientMessageInspector
    {
        private ILogger<LoggingMessageInspector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingMessageInspector"/> class.
        /// </summary>
        /// <param name="logger">The logger provider.</param>
        public LoggingMessageInspector(ILogger<LoggingMessageInspector> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implementation of IClientMessageInspector
        /// Gets called AFTER receiving a reply from the Soap Call.
        /// </summary>
        /// <param name="reply">The reply message.</param>
        /// <param name="correlationState">Correlation State.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            Contract.Requires(reply != null);
            this.logger.LogDebug($"Getting the reply response... {reply.State}");
            using (MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue))
            {
                XmlDocument document = this.GetDocument(buffer.CreateMessage());
                this.logger.LogDebug($"Finished getting the reply response. {document.OuterXml}");
                reply = buffer.CreateMessage();
            }
        }

        /// <summary>
        /// Implementation of IClientMessageInspector
        /// Gets called BEFORE receiving a reply from the Soap Call.
        /// </summary>
        /// <param name="request">The request message to be send.</param>
        /// <param name="channel">The client channel.</param>
        /// <returns>The object that is returned as the correlationState argument of the AfterReceiveReply(Message, Object) method.</returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            Contract.Requires(request != null);
            this.logger.LogDebug($"Getting the reply request... {request.State}");

            using (var buffer = request.CreateBufferedCopy(int.MaxValue))
            {
                var document = this.GetDocument(buffer.CreateMessage());
                this.logger.LogDebug($"Finished getting the reply request. {document.OuterXml}");
                request = buffer.CreateMessage();
                return null;
            }
        }

        private XmlDocument GetDocument(Message request)
        {
            XmlDocument document = new XmlDocument();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // write request to memory stream
                using (XmlWriter writer = XmlWriter.Create(memoryStream))
                {
                    request.WriteMessage(writer);
                    writer.Flush();
                }

                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}