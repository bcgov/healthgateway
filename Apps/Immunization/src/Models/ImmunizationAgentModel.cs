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
namespace HealthGateway.Immunization.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an immunization agent returned by the V2 API.
    /// </summary>
    public class ImmunizationAgentModel
    {
        /// <summary>
        /// Gets or sets the agent code.
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the agent name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the lot number.
        /// </summary>
        [JsonPropertyName("lotNumber")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }

        /// <summary>
        /// Gets or sets the system.
        /// </summary>
        [JsonPropertyName("system")]
        public string? System { get; set; }
    }
}
