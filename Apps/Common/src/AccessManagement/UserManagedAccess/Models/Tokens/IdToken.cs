//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace HealthGateway.Common.UserManagedAccess.Models.Tokens
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 Access Token Response json.
    /// </summary>
    public class IdToken : JwtPayload
    {
        /// <summary>
        /// Gets the session_state.
        /// </summary>
        [JsonPropertyName("session_state")]
        public string? SessionState { get; }

        /// <summary>
        /// Gets the at_hash.
        /// </summary>
        [JsonPropertyName("at_hash")]
        public string? AccessTokenHash { get; }

        /// <summary>
        /// Gets the c_hash.
        /// </summary>
        [JsonPropertyName("c_hash")]
        public string? CodeHash { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; }

        /// <summary>
        /// Gets the given_name.
        /// </summary>
        [JsonPropertyName("given_name")]
        public string? GivenName { get; }

        /// <summary>
        /// Gets the family_name.
        /// </summary>
        [JsonPropertyName("family_name")]
        public string? FamilyName { get; }

        /// <summary>
        /// Gets the middle_name.
        /// </summary>
        [JsonPropertyName("middle_name")]
        public string? MiddleName { get; }

        /// <summary>
        /// Gets the nickname.
        /// </summary>
        [JsonPropertyName("nickname")]
        public string? NickName { get; }

        /// <summary>
        /// Gets the preferred_username.
        /// </summary>
        [JsonPropertyName("preferred_username")]
        public string? PreferredUsername { get; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        [JsonPropertyName("profile")]
        public string? Profile { get; }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        [JsonPropertyName("picture")]
        public string? Picture { get; }

        /// <summary>
        /// Gets the website.
        /// </summary>
        [JsonPropertyName("website")]
        public string? Website { get; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; }

        /// <summary>
        /// Gets a value indicating whether the email_verified flag is set.
        /// </summary>
        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; } = false;

        /// <summary>
        /// Gets the gender.
        /// </summary>
        [JsonPropertyName("gender")]
        public string? Gender { get; }

        /// <summary>
        /// Gets the birthdate.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public string? Birthdate { get; }

        /// <summary>
        /// Gets the zoneinfo.
        /// </summary>
        [JsonPropertyName("zoneinfo")]
        public string? Zoneinfo { get; }

        /// <summary>
        /// Gets the locale.
        /// </summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; }

        /// <summary>
        /// Gets the phone_number.
        /// </summary>
        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; }

        /// <summary>
        /// Gets a value indicating whether the phone_number_verified flag is set.
        /// </summary>
        [JsonPropertyName("phone_number_verified")]
        public bool PhoneNumberVerified { get; } = false;

        /// <summary>
        /// Gets a value for the address as an <see cref="AddressClaimSet"/>.
        /// </summary>
        [JsonPropertyName("address")]
        public AddressClaimSet? Address { get; }

        /// <summary>
        /// Gets a value for the updated_at timestamp (long).
        /// </summary>
        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; }

        /// <summary>
        /// Gets the claims_locales.
        /// </summary>
        [JsonPropertyName("claims_locales")]
        public string? ClaimsLocales { get; }

        /// <summary> Gets the s_hash. Financial API - Part 2: Read and Write API Security Profile
        /// http://openid.net/specs/openid-financial-api-part-2.html#authorization-server.</summary>
        [JsonPropertyName("s_hash")]
        public string? StateHash { get; }
    }
}