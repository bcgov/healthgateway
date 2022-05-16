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
namespace HealthGateway.Patient.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents an address.
/// </summary>
public class Address
{
        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Address"/> class.
        /// </summary>
        /// <param name="address">The address model object from patient service model.</param>
        public Address(Common.Models.Address address)
        {
            this.StreetLines = address.StreetLines;
            this.City = address.City;
            this.State = address.State;
            this.PostalCode = address.PostalCode;
            this.Country = address.Country;
        }

        /// <summary>
        /// Gets the street lines.
        /// </summary>
        [JsonPropertyName("streetLines")]
        public IList<string> StreetLines { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state/province.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

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
