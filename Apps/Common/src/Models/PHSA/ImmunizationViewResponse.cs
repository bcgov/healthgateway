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
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA Immunization view data model.
    /// </summary>
    public class ImmunizationViewResponse
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the SourceSystemId.
        /// </summary>
        [JsonPropertyName("sourceSystemId")]
        public string SourceSystemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the Immunization is valid.
        /// </summary>
        [JsonPropertyName("valid")]
        public bool Valid { get; set; } = false;

        /// <summary>
        /// Gets or sets the Provider or Clinic providing the Immunization.
        /// </summary>
        [JsonPropertyName("providerOrClinic")]
        public string ProviderOrClinic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Targeted Disease of the Immunization.
        /// </summary>
        [JsonPropertyName("targetedDisease")]
        public string TargetedDisease { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization occurence date time.
        /// </summary>
        [JsonPropertyName("occurrenceDateTime")]
        public DateTime OccurrenceDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Immunization Forecasts.
        /// </summary>
        [JsonPropertyName("immunizationForecast")]
        public ImmunizationForecastResponse? ImmunizationForecast { get; set; } = null;

        /// <summary>
        /// Gets or sets the Immunization Agents.
        /// </summary>
        [JsonPropertyName("immunizationAgents")]
        public IEnumerable<ImmunizationAgentResponse> ImmunizationAgents { get; set; } = new List<ImmunizationAgentResponse>();
    }
}
