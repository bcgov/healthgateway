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
namespace HealthGateway.Admin.Server.Models.Immunization
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Models.PHSA;

    /// <summary>
    /// The PHSA vaccine details data model.
    /// </summary>
    public class VaccineDetailsResponse
    {
        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        [JsonPropertyName("verificationDob")]
        public DateTime? VerificationBirthdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested record has been protected from being accessed.
        /// </summary>
        [JsonPropertyName("blocked")]
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested record contains any invalid doses.
        /// </summary>
        [JsonPropertyName("containsInvalidDoses")]
        public bool ContainsInvalidDoses { get; set; }

        /// <summary>
        /// Gets or sets the patient's dose dates from the Provincial Immunization Registry (Panorama).
        /// </summary>
        [JsonPropertyName("pirLookupDoseDates")]
        public IEnumerable<DateTime> PirLookupDoseDates { get; set; } = [];

        /// <summary>
        /// Gets or sets the patient's dose dates from Immunize BC.
        /// </summary>
        [JsonPropertyName("immBcLookupDoseDates")]
        public IEnumerable<DateTime> ImmBcLookupDoseDates { get; set; } = [];

        /// <summary>
        /// Gets or sets the data from the enterprise-wide master patient index.
        /// </summary>
        [JsonPropertyName("empiData")]
        public EmpiDataResponse? EmpiDataResponse { get; set; } = null;

        /// <summary>
        /// Gets or sets the Vaccine Status Result.
        /// </summary>
        [JsonPropertyName("vaccineStatusIndicator")]
        public VaccineStatusResult? VaccineStatusResult { get; set; } = null;

        /// <summary>
        /// Gets or sets the patient's doses.
        /// </summary>
        [JsonPropertyName("doses")]
        public IEnumerable<VaccineDoseResponse> Doses { get; set; } = [];
    }
}
