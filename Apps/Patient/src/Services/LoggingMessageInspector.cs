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
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using Microsoft.Extensions.Logging;
    using System.Xml;
    using System.IO;

    /// <summary>
    /// Implementation of IClientMessageInspector for loging purposes
    /// </summary>
    public class LoggingMessageInspector : IClientMessageInspector
    {
        private ILogger<LoggingMessageInspector> logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public LoggingMessageInspector(ILogger<LoggingMessageInspector> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implementation of IClientMessageInspector
        /// Gets called AFTER receiving a reply from the Soap Call
        /// </summary>
        /// <param name="reply">The reply message.</param>
        /// <param name="correlationState">Correlation State.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            using (var buffer = reply.CreateBufferedCopy(int.MaxValue))
            {
                var document = GetDocument(buffer.CreateMessage());
                this.logger.LogTrace(document.OuterXml);

                reply = buffer.CreateMessage();
            }
        }

        /// <summary>
        /// Implementation of IClientMessageInspector
        /// Gets called BEFORE receiving a reply from the Soap Call
        /// </summary>
        /// <param name="request">The request message to be send.</param>
        /// <param name="channel">The request message to be send.</param>
        /// <returns>The object that is returned as the correlationState argument of the AfterReceiveReply(Message, Object) method.</returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            using (var buffer = request.CreateBufferedCopy(int.MaxValue))
            {
                var document = GetDocument(buffer.CreateMessage());
                this.logger.LogTrace(document.OuterXml);

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
                XmlWriter writer = XmlWriter.Create(memoryStream);
                request.WriteMessage(writer);
                writer.Flush();
                memoryStream.Position = 0;

                // load memory stream into a document
                document.Load(memoryStream);
            }

            return document;
        }
    }
}