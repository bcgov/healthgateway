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
namespace HealthGateway.GatewayApi.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Configuration to be used by external clients for authentication.
    /// </summary>
    public class OpenIdConnectConfiguration
    {
        /// <summary>
        /// Gets or sets the OpenIDConnect Authority.
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OpenIdConnect Client ID.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Audience.
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OpenIDConnect Response types.
        /// </summary>
        public string ResponseType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OpenIDConnect Scopes.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets the Callback URIs.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, System.Uri>? Callbacks { get; set; }
    }
}
