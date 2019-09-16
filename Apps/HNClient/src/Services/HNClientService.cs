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
        private readonly string TimeRequest;

        public HNClientService(IConfiguration configuration,IHNClientDelegate hnclient)
        {
            this.hnclient = hnclient;
            this.configuration = configuration;
            this.TimeRequest = configuration.GetSection("HNClient").GetValue<string>("TimeMessage");
        }

        public TimeMessage GetTime()
        {
            Message msg = this.SendMessage(TimeRequest);
            TimeMessage retMessage = new TimeMessage
            {
                IsErr = msg.IsErr,
                Error = msg.Error,
                Reply = msg.Reply,
            };
            retMessage.DateTime = System.DateTime.Now;

            // TODO extract the datetime instead of defaulting.
            return retMessage;
        }

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
