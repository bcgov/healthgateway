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
namespace HealthGateway.Admin.Models.Support
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a dose of the report.
    /// </summary>
    public class ReportDose
    {
        /// <summary>
        /// Gets or sets the dose number.
        /// </summary>
        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dose product.
        /// </summary>
        [JsonPropertyName("product")]
        public string Product { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dose immunizing agent.
        /// </summary>
        [JsonPropertyName("immunizingAgent")]
        public string ImmunizingAgent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dose provider.
        /// </summary>
        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dose date.
        /// </summary>
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dose lot number.
        /// </summary>
        [JsonPropertyName("lotNumber")]
        public string LotNumber { get; set; } = string.Empty;
    }
}
