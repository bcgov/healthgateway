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
    /// The Vaccine Status model.
    /// </summary>
    public class VaccineStatusResult
    {
        /// <summary>
        /// Gets or sets the patient's first name.
        /// </summary>
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the patient's last name.
        /// </summary>
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        [JsonPropertyName("dob")]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Gets or sets one of the patient's vaccine dates.
        /// </summary>
        [JsonPropertyName("vaccinedate")]
        public DateTime? VaccineDate { get; set; }

        /// <summary>
        /// Gets or sets the number of doses of the vaccine that have been administered to the identified PHN.
        /// </summary>
        [JsonPropertyName("doseCount")]
        public int DoseCount { get; set; }

        /// <summary>
        /// Gets or sets the vaccine state.
        /// </summary>
        [JsonPropertyName("statusIndicator")]
        public string StatusIndicator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the QR code associated with the Vaccine Status.
        /// </summary>
        [JsonPropertyName("qrCode")]
        public EncodedMedia QRCode { get; set; } = new();

        /// <summary>
        /// Gets or sets the Proof of Vaccination associated with the Vaccine Status.
        /// </summary>
        [JsonPropertyName("federalPvc")]
        public EncodedMedia? FederalVaccineProof { get; set; } = new();
    }
}
