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
namespace HealthGateway.Medication.Parsers
{
    using System.Collections.Generic;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The HNClient parser interface.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public interface IHNMessageParser<T>
    {
        /// <summary>
        /// Creates a request message to HNClient.
        /// </summary>
        /// <param name="id">The lookup id.</param>
        /// <param name="userId">The requester user id.</param>
        /// <param name="ipAddress">The requester ip address.</param>
        /// <returns>The HL7 message.</returns>
        HNMessage<string> CreateRequestMessage(string id, string userId, string ipAddress);

        /// <summary>
        /// Parses a response message from HNClient.
        /// </summary>
        /// <param name="hl7Message">The raw hl7 message.</param>
        /// <returns>The model list.</returns>
        HNMessage<T> ParseResponseMessage(string hl7Message);
    }
}
