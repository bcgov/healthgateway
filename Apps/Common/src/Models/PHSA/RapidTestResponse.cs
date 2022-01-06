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
namespace HealthGateway.Common.Models.PHSA
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The representation of a rapid test result for authenticated access.
    /// </summary>
    public class RapidTestResponse
    {
        /// <summary>
        /// Gets or sets the client Phn.
        /// </summary>
        [JsonPropertyName("phn")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Rapid Test Results.
        /// </summary>
        [JsonPropertyName("c19RapidTestResults")]
        public IEnumerable<RapidTestResult> RapidTestResults { get; set; } = new List<RapidTestResult>();
    }
}
