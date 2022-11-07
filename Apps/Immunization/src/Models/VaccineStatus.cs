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
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// The Vaccine Status model.
    /// </summary>
    public class VaccineStatus
    {
        /// <summary>
        /// Gets or sets a value indicating the ID to be used to fetch the vaccine status.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the VaccineStatus has been retrieved.
        /// Will be set to true if the object has been fully loaded.
        /// When false, only ID, Loaded, and RetryIn will be populated.
        /// </summary>
        [JsonPropertyName("loaded")]
        public bool Loaded { get; set; }

        /// <summary>
        /// Gets or sets the minimal amount of time that should be waited before another request.
        /// The unit of measurement is in milliseconds.
        /// </summary>
        [JsonPropertyName("retryin")]
        public int RetryIn { get; set; }

        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        [JsonPropertyName("personalhealthnumber")]
        public string? PersonalHealthNumber { get; set; }

        /// <summary>
        /// Gets or sets the patient's first name.
        /// </summary>
        [JsonPropertyName("firstname")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the patient's last name.
        /// </summary>
        [JsonPropertyName("lastname")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Gets or sets one of the patient's vaccine dates.
        /// </summary>
        [JsonPropertyName("vaccinedate")]
        public DateTime? VaccineDate { get; set; }

        /// <summary>
        /// Gets or sets the number of doses of the vaccine that have been administered to the identified PHN.
        /// </summary>
        [JsonPropertyName("doses")]
        public int Doses { get; set; }

        /// <summary>
        /// Gets or sets the vaccine state.
        /// </summary>
        [JsonPropertyName("state")]
        public VaccineState State { get; set; }

        /// <summary>
        /// Gets or sets the QR code.
        /// </summary>
        [JsonPropertyName("qrCode")]
        public EncodedMedia QRCode { get; set; } = new();

        /// <summary>
        /// Gets or sets the Federal Proof of Vaccination document.
        /// </summary>
        [JsonPropertyName("federalVaccineProof")]
        public EncodedMedia? FederalVaccineProof { get; set; } = new();
    }
}
