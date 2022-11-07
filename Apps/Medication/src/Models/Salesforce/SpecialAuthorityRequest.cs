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
namespace HealthGateway.Medication.Models.Salesforce
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Special Authority Request.
    /// </summary>
    public class SpecialAuthorityRequest
    {
        /// <summary>
        /// Gets or sets the patientIdentifier.
        /// </summary>
        [JsonPropertyName("patientIdentifier")]
        public string? PatientIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the patientFirstName.
        /// </summary>
        [JsonPropertyName("patientFirstName")]
        public string? PatientFirstName { get; set; }

        /// <summary>
        /// Gets or sets the patientLastName.
        /// </summary>
        [JsonPropertyName("patientLastName")]
        public string? PatientLastName { get; set; }

        /// <summary>
        /// Gets or sets the drugName.
        /// </summary>
        [JsonPropertyName("drugName")]
        public string? DrugName { get; set; }

        /// <summary>
        /// Gets or sets the request status.
        /// </summary>
        [JsonPropertyName("requestStatus")]
        public string? RequestStatus { get; set; }

        /// <summary>
        /// Gets or sets the prescriber's firstname.
        /// </summary>
        [JsonPropertyName("prescriberFirstName")]
        public string? PrescriberFirstName { get; set; }

        /// <summary>
        /// Gets or sets the prescriber's lastname.
        /// </summary>
        [JsonPropertyName("prescriberLastName")]
        public string? PrescriberLastName { get; set; }

        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        [JsonPropertyName("referenceNumber")]
        public string ReferenceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        [JsonPropertyName("effectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        [JsonPropertyName("expiryDate")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the requested date.
        /// </summary>
        [JsonPropertyName("requestedDate")]
        public DateTime RequestedDate { get; set; }
    }
}
