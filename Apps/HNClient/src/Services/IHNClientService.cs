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

namespace HealthGateway.HNClient.Services
{
    using System;
    using HealthGateway.HNClient.Models;

    /// <summary>
    /// Interface for the HNClient Service proxy.
    /// </summary>
    public interface IHNClientService
    {
        /// <summary>
        /// Requests the time from the HNSecure infrastructure.
        /// </summary>
        /// <remarks>
        /// Equivalent to Posting message: <![CDATA[MSH|^~\&|HNTIMEAP||HNETDTTN|BC00001000|20190101120000+0800|GATEWAY|NMQ||D|2.3]]>.
        /// to SendMessage.
        /// </remarks>
        /// <returns>A TimeMessage object containing the HNSecure time along with the raw data.</returns>
        HNMessage GetTime();

        /// <summary>
        /// Sends an arbitrary HL7 2.3 message to HNSecure.
        /// </summary>
        /// <remarks>
        /// Sample message: <![CDATA[MSH|^~\&|HNTIMEAP||HNETDTTN|BC00001000|20190101120000+0800|GATEWAY|NMQ||D|2.3]]>.
        /// </remarks>
        /// <param name="msg">The HL7 V2.3 message to send.</param>
        /// <returns>A message with the embedded response or error message.</returns>
        HNMessage SendMessage(HNMessage msg);
    }
}
