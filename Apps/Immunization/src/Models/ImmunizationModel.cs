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
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an immunization record returned by the V2 API.
    /// </summary>
    public class ImmunizationModel
    {
        /// <summary>
        /// Gets or sets the immunization id.
        /// </summary>
        [JsonPropertyName("immunizationId")]
        public string? ImmunizationId { get; set; }

        /// <summary>
        /// Gets or sets the vaccine name.
        /// </summary>
        [JsonPropertyName("vaccineName")]
        public string? VaccineName { get; set; }

        /// <summary>
        /// Gets or sets the immunization status.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date time.
        /// </summary>
        [JsonPropertyName("occurrenceDateTime")]
        public DateTime? OccurrenceDateTime { get; set; }

        /// <summary>
        /// Gets or sets the provider or clinic.
        /// </summary>
        [JsonPropertyName("providerOrClinic")]
        public string? ProviderOrClinic { get; set; }

        /// <summary>
        /// Gets or sets the immunization agents.
        /// </summary>
        [JsonPropertyName("agents")]
        public IEnumerable<ImmunizationAgentModel> Agents { get; set; } = [];

        /// <summary>
        /// Gets or sets the immunization forecast.
        /// </summary>
        [JsonPropertyName("forecast")]
        public ImmunizationForecastModel? Forecast { get; set; }
    }
}
