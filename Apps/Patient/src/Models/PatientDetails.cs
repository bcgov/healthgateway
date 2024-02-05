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
namespace HealthGateway.Patient.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.AccountDataAccess.Patient;

    /// <summary>
    /// The patient details.
    /// </summary>
    public class PatientDetails
    {
        /// <summary>
        /// Gets or sets the health directed identifier.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        [JsonPropertyName("personalHealthNumber")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's common name.
        /// </summary>
        [JsonPropertyName("commonName")]
        public Name? CommonName { get; set; }

        /// <summary>
        /// Gets or sets the patient's legal name.
        /// </summary>
        [JsonPropertyName("legalName")]
        public Name? LegalName { get; set; }

        /// <summary>
        /// Gets the patient's preferred name.
        /// </summary>
        [JsonPropertyName("preferredName")]
        public Name PreferredName => this.CommonName ?? this.LegalName!;

        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateOnly Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the patient's gender.
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the physical address for the patient.
        /// </summary>
        public Address? PhysicalAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal address for the patient.
        /// </summary>
        public Address? PostalAddress { get; set; }

        /// <summary>
        /// Gets or sets the response code for the patient.
        /// </summary>
        public string ResponseCode { get; set; } = string.Empty;
    }
}
