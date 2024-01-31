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
namespace HealthGateway.Common.AccessManagement.Authentication.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The json web token model.
    /// </summary>
    public class JwtModel : IAuthModel
    {
        /// <inheritdoc/>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("refresh_expires_in")]
        public int? RefreshExpiresIn { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("not-before-policy")]
        public int? NotBeforePolicy { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("session_state")]
        public string? SessionState { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
