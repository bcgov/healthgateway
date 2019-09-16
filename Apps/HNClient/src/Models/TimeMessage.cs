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
namespace HealthGateway.HNClient.Models
{
    using System;

    /// <summary>
    /// A simple model object representing a Time response and error state from HNSecure.
    /// </summary>
    public class TimeMessage : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMessage"/> class.
        /// </summary>
        /// <param name="baseMsg">The base message object.</param>
        public TimeMessage(Message baseMsg)
        {
            if (baseMsg is null)
            {
                throw new ArgumentNullException(nameof(baseMsg));
            }

            this.IsErr = baseMsg.IsErr;
            this.Error = baseMsg.Error;
            this.Reply = baseMsg.Reply;

            // Sample message: MSH|^~\\&|HNETDTTN|BC00001000|HNTIMEAP|BC01001239|20190916133152-0800|GATEWAY|NMR||D|2.3||||\rMSA|AA|||||\rNCK|20190916133152-0800\r\r\r
            if (!this.IsErr)
            {
                // Parses the response message
                string[] cols = this.Reply.Split("|");
                this.DateTime = DateTime.ParseExact(cols[6], "yyyyMMddHHmmssK", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the Date and Time from the HNServer.
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}
