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
namespace namespace HealthGateway.Common.UserManagedAccess.Models.Tokens
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 Access Token Response json.
    /// </summary>
    public class AccessToken : JwtPayload
    {
        /// <summary>
        /// Gets the truested-certs.
        /// </summary>
        [JsonPropertyName("trusted-certs")]
        public List<string>? TrustedCertificates { get; } = new List<string>();

        /// <summary>
        /// Gets the allowed-origins.
        /// </summary>
        [JsonPropertyName("allowed-origins")]
        public List<string>? AllowedOrigins { get; } = new List<string>();

        /// <summary>
        /// Gets the realm_access.
        /// </summary>
        [JsonPropertyName("realm_access")]
        public Access? RealmAccess { get; }

        /// <summary>
        /// Gets the resource_access.
        /// </summary>
        [JsonPropertyName("resource_access")]
        public Dictionary<string, Access> ResourceAccess { get; } = new Dictionary<string, Access>();

        /// <summary>
        /// Gets the authorization.
        /// </summary>
        [JsonPropertyName("authorization")]
        public ResourceAuthorization? ResourceAuthorization { get; }

        /// <summary>
        /// Gets the cnf.
        /// </summary>
        [JsonPropertyName("cnf")]
        public CertConf? CertConf { get; }

        /// <summary>
        /// Gets  the scope.
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; }
    }
}