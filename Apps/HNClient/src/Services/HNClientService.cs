// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.HNClient.Services
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using HealthGateway.HNClient.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A simple proxy service to HNClient.
    /// </summary>
    public class HNClientService : IHNClientService
    {
        private readonly IHNClientDelegate hnclient;
        private readonly IConfiguration configuration;
        private readonly string timeRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HNClientService"/> class.
        /// </summary>
        /// <param name="configuration">The injected IConfiguration object.</param>
        /// <param name="hnclient">The injected HNClient delegate.</param>
        public HNClientService(IConfiguration configuration, IHNClientDelegate hnclient)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.hnclient = hnclient;
            this.configuration = configuration;
            this.timeRequest = configuration.GetSection("HNClient").GetValue<string>("TimeMessage");
        }

        /// <summary>
        /// Requests the time from the HNSecure infrastructure.
        /// </summary>
        /// <remarks>
        /// Equivalent to Posting message: <![CDATA[MSH|^~\&|HNTIMEAP||HNETDTTN|BC00001000|20190101120000+0800|GATEWAY|NMQ||D|2.3]]>.
        /// to SendMessage.
        /// </remarks>
        /// <returns>A TimeMessage object containing the HNSecure time along with the raw data.</returns>
        public TimeMessage GetTime()
        {
            Message msg = this.SendMessage(this.timeRequest);
            return new TimeMessage(msg);
        }

        /// <summary>
        /// Sends an arbitrary HL7 2.3 message to HNSecure.
        /// </summary>
        /// <remarks>
        /// Sample message: <![CDATA[MSH|^~\&|HNTIMEAP||HNETDTTN|BC00001000|20190101120000+0800|GATEWAY|NMQ||D|2.3]]>.
        /// </remarks>
        /// <param name="msg">The HL7 V2.3 message to send.</param>
        /// <returns>A message with the embedded response or error message.</returns>
        public Message SendMessage(string msg)
        {
            Message retMessage = new Message();
            try
            {
                retMessage.Reply = this.hnclient.SendReceive(msg);
            }
            catch (Exception e)
            {
                if (e is InvalidDataException ||
                    e is InvalidOperationException ||
                    e is SocketException)
                {
                    retMessage.IsErr = true;
                    retMessage.Error = e.Message;
                }
                else
                {
                    throw e;
                }
            }

            return retMessage;
        }
    }
}
