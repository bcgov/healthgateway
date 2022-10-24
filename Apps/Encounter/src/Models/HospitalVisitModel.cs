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
namespace HealthGateway.Encounter.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Hospital Visit.
    /// </summary>
    public class HospitalVisitModel
    {
        /// <summary>
        /// Gets or sets the encounter id.
        /// </summary>
        [JsonPropertyName("encounterId")]
        public string? EncounterId { get; set; }

        /// <summary>
        /// Gets or sets the facility.
        /// </summary>
        [JsonPropertyName("facility")]
        public string? Facility { get; set; }

        /// <summary>
        /// Gets or sets the health service.
        /// </summary>
        [JsonPropertyName("healthService")]
        public string? HealthService { get; set; }

        /// <summary>
        /// Gets or sets the visit type.
        /// </summary>
        [JsonPropertyName("visitType")]
        public string? VisitType { get; set; }

        /// <summary>
        /// Gets or sets the health authority.
        /// </summary>
        [JsonPropertyName("healthAuthority")]
        public string? HealthAuthority { get; set; }

        /// <summary>
        /// Gets or sets the admit date time.
        /// </summary>
        [JsonPropertyName("admitDateTime")]
        public DateTime? AdmitDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        [JsonPropertyName("endDateTime")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }
    }
}
