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
    /// OAuth2 OIDC Client Credentials Grant Token request information.
    /// </summary>
    public class ClientCredentialsTokenRequest
    {
        /// <summary>
        /// Gets or sets a unique identifier of the target API you want to access.
        /// </summary>
        [JsonPropertyName("audience")]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets scopes for the access.
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the application's Client ID.
        /// </summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets your application's Client Secret.
        /// </summary>
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource owner username for OAuth 2 Rsource Owner Password Grant.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource owner password for OAuth 2 Rsource Owner Password Grant.
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}