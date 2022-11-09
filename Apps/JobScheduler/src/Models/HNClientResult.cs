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
namespace HealthGateway.JobScheduler.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the RocketChat Configuration.
    /// </summary>
    public class HNClientResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HNClientResult"/> class.
        /// </summary>
        public HNClientResult()
        {
        }

        /// <summary>
        /// Gets or sets the HL7 Message.
        /// </summary>
        [JsonPropertyName("message")]
        public string? HL7Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error has occurred.
        /// </summary>
        [JsonPropertyName("isErr")]
        public bool Error { get; set; }

        /// <summary>
        /// Gets or sets the Error Message.
        /// </summary>
        [JsonPropertyName("error")]
        public string? ErrorMessage { get; set; }
    }
}
