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
    using HealthGateway.HNClient.Models;

    /// <summary>
    /// A simple proxy service to HNClient.
    /// </summary>
    public class HNClientService : IHNClientService
    {
        private IHNClientDelegate hnclient;

        public HNClientService(IHNClientDelegate hnclient)
        {
            this.hnclient = hnclient;
        }

        public TimeMessage GetTime()
        {
            Message msg = this.SendMessage("MSH");
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
            catch (InvalidDataException e)
            {
                retMessage.IsErr = true;
                retMessage.Error = e.Message;
            }

            return retMessage;
        }
    }
}
