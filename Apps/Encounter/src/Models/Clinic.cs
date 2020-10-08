//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Encounter.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The Location Address data model.
    /// </summary>
    public class Clinic
    {
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 1.
        /// </summary>
        [JsonPropertyName("addressLine1")]
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 2.
        /// </summary>
        [JsonPropertyName("addressLine2")]
        public string AddressLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 3.
        /// </summary>
        [JsonPropertyName("addressLine3")]
        public string AddressLine3 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 4.
        /// </summary>
        [JsonPropertyName("addressLine4")]
        public string AddressLine4 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Postal Code.
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Province.
        /// </summary>
        [JsonPropertyName("province")]
        public string Province { get; set; } = string.Empty;
    }
}
