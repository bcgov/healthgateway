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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Immunization.Constants;

    /// <summary>
    /// The Vaccine Status Report Request model.
    /// </summary>
    public class VaccineStatusReportRequest
    {
        /// <summary>
        /// Gets or sets the patient's first name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public string Birthdate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's status.
        /// </summary>
        [JsonPropertyName("status")]
        public VaccineState Status { get; set; }

        /// <summary>
        /// Gets or sets the patient's doses.
        /// </summary>
        [JsonPropertyName("doses")]
        public int Doses { get; set; }
    }
}
