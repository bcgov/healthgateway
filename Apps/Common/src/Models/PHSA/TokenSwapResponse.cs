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
namespace HealthGateway.Common.Models.PHSA
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model object representing an access token.
    /// </summary>
    public class TokenSwapResponse
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expires on for the token.
        /// </summary>
        [JsonPropertyName("expires_on")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the token type for the token.
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scope for the token.
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}
