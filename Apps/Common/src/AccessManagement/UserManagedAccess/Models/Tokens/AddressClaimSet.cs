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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models.Tokens
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The Address Claims as may be part of an IdToken.
    /// </summary>
    public class AddressClaimSet
    {
        /// <summary>
        /// Gets the Formatted address.
        /// </summary>
        [JsonPropertyName("formatted")]
        public string? FormattedAddress { get; }

        /// <summary>
        /// Gets the street address.
        /// </summary>
        [JsonPropertyName("street_address")]
        public string? StreetAddress { get; }

        /// <summary>
        /// Gets the locality.
        /// </summary>
        [JsonPropertyName("locality")]
        public string? Locality { get; }

        /// <summary>
        /// Gets the region.
        /// </summary>
        [JsonPropertyName("region")]
        public string? Region { get; }

        /// <summary>
        /// Gets the postal code.
        /// </summary>
        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; }

        /// <summary>
        /// Gets the country.
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; }
    }
}
