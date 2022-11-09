// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Medication.Models.ODR
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The ProtectiveWord Request/Response model.
    /// </summary>
    public class ProtectiveWord
    {
        /// <summary>
        /// Gets or sets the Id of the request.
        /// </summary>
        [JsonPropertyName("uuid")]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the HDID of the requestor.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string RequestorHdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the IP of the requestor.
        /// </summary>
        [JsonPropertyName("requestingIP")]
        public string RequestorIp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the QueryRequest for the MedicationHistory integration.
        /// </summary>
        [JsonPropertyName("maintainProtectiveWord")]
        public ProtectiveWordQueryResponse? QueryResponse { get; set; }
    }
}
