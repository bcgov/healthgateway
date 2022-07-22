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
    public class LocationAddress
    {
        /// <summary>
        /// Gets or sets the Address Line 1.
        /// </summary>
        [JsonPropertyName("addrLine1")]
        public string AddrLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 2.
        /// </summary>
        [JsonPropertyName("addrLine2")]
        public string AddrLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 3.
        /// </summary>
        [JsonPropertyName("addrLine3")]
        public string AddrLine3 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 4.
        /// </summary>
        [JsonPropertyName("addrLine4")]
        public string AddrLine4 { get; set; } = string.Empty;

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
