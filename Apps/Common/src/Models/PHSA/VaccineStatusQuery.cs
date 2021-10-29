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
    using System.Text.Json.Serialization;

    /// <summary>
    /// The Vaccine Status Query model.
    /// </summary>
    public class VaccineStatusQuery
    {
        /// <summary>
        /// Gets or sets the patient hdid.
        /// </summary>
        [JsonIgnore]
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Personal Health Number.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the person's date of birth.
        /// </summary>
        [JsonPropertyName("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets one of the person's vaccine dates.
        /// </summary>
        [JsonPropertyName("dateOfVaccination")]
        public DateTime DateOfVaccine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Federal Proof of Vaccination document should be included.
        /// </summary>
        [JsonPropertyName("federalPvc")]
        public bool IncludeFederalVaccineProof { get; set; }
    }
}
