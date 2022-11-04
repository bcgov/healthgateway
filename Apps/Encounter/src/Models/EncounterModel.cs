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
    /// Represents a patient Encounter.
    /// </summary>
    public class EncounterModel
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the EncounterDate.
        /// </summary>
        [JsonPropertyName("encounterDate")]
        public DateTime EncounterDate { get; set; }

        /// <summary>
        /// Gets or sets the Specialty Description.
        /// </summary>
        [JsonPropertyName("specialtyDescription")]
        public string SpecialtyDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Practitioner Name.
        /// </summary>
        [JsonPropertyName("practitionerName")]
        public string PractitionerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Location Name.
        /// </summary>
        [JsonPropertyName("clinic")]
        public Clinic Clinic { get; set; } = new();
    }
}
