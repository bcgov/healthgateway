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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Pharmacy model.
    /// </summary>
    public class Pharmacy
    {
        /// <summary>
        /// Gets or sets the Pharmacy ID.
        /// </summary>
        [JsonPropertyName("pharmacyId")]
        public string PharmacyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the Pharmacy.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address for the pharmacy.
        /// </summary>
        [JsonPropertyName("siteAddress")]
        public Address Address { get; set; } = new();

        /// <summary>
        /// Gets or sets the Pharmacy's Phone number.
        /// </summary>
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Pharmacy's Fax Number.
        /// </summary>
        [JsonPropertyName("faxNumber")]
        public string FaxNumber { get; set; } = string.Empty;
    }
}
