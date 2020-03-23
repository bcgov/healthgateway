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
    /// Address model.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets Line1 of the address.
        /// </summary>
        [JsonPropertyName("addrLine1")]
        public string Line1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Line2 of the address.
        /// </summary>
        [JsonPropertyName("addrLine2")]
        public string Line2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets City for the address.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Province for the address.
        /// </summary>
        [JsonPropertyName("province")]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets PostalCode for the address.
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Country for the address.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
