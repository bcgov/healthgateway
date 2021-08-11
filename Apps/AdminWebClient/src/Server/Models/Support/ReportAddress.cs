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
namespace HealthGateway.Admin.Models.Support
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the report address.
    /// </summary>
    public class ReportAddress
    {
        /// <summary>
        /// Gets or sets the name of the person on the address.
        /// </summary>
        [JsonPropertyName("addressee")]
        public string Addressee { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the street name.
        /// </summary>
        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the province or state.
        /// </summary>
        [JsonPropertyName("provinceOrState")]
        public string ProvinceOrState { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        [JsonPropertyName("code")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
