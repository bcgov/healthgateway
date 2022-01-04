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
namespace HealthGateway.Laboratory.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the result from submitting an authenticated COVID-19 rapid test.
    /// </summary>
    public class AuthenticateRapidTestResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateRapidTestResponse"/> class.
        /// </summary>
        public AuthenticateRapidTestResponse()
        {
            this.Records = new List<RapidTestRecord>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticateRapidTestResponse"/> class.
        /// </summary>
        /// <param name="records">The list of COVID-19 rapid tests.</param>
        public AuthenticateRapidTestResponse(IList<RapidTestRecord> records)
        {
            this.Records = records;
        }

        /// <summary>
        /// Gets or sets the client PHN.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rapid tests.
        /// </summary>
        [JsonPropertyName("c19RapidTestResults")]
        public IEnumerable<RapidTestRecord> Records { get; set; }
    }
}
