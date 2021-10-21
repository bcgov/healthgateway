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
namespace HealthGateway.Common.Models.BCMailPlus
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an address to be sent to BC Mail Plus.
    /// </summary>
    public class BcmpAddress
    {
        /// <summary>
        /// Gets or sets the first address line.
        /// </summary>
        [JsonPropertyName("addr1")]
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the second address line.
        /// </summary>
        [JsonPropertyName("addr2")]
        public string AddressLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state/province.
        /// </summary>
        [JsonPropertyName("province")]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
