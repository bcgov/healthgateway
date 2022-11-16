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
namespace HealthGateway.Medication.Models.ODR
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a dispensed prescription.
    /// </summary>
    public class MedicationResult
    {
        /// <summary>
        /// Gets or sets the unique identifer for this record.
        /// </summary>
        [JsonPropertyName("recordId")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the dispenseDate.
        /// </summary>
        [JsonPropertyName("dateDispensed")]
        public DateTime DispenseDate { get; set; }

        /// <summary>
        /// Gets or sets the prescription number.
        /// </summary>
        [JsonPropertyName("rxNumber")]
        public string PrescriptionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the prescription status.
        /// </summary>
        [JsonPropertyName("rxStatus")]
        public string PrescriptionStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the DIN/PIN of the prescription.
        /// </summary>
        [JsonPropertyName("dinPin")]
        public string Din { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Generic name for the prescription.
        /// </summary>
        [JsonPropertyName("genericName")]
        public string GenericName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the prescription.
        /// </summary>
        [JsonPropertyName("quantity")]
        public float Quantity { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Refills for the prescription.
        /// </summary>
        [JsonPropertyName("refills")]
        public int Refills { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Directions given for the prescription.
        /// </summary>
        [JsonPropertyName("directions")]
        public string Directions { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Practioners name.
        /// </summary>
        [JsonPropertyName("practitioner")]
        public Name? Practioner { get; set; }

        /// <summary>
        /// Gets or sets the Dispensing Pharmacy of the prescription.
        /// </summary>
        [JsonPropertyName("dispensingPharmacy")]
        public Pharmacy? DispensingPharmacy { get; set; }
    }
}
