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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Configuration to be used by external clients for authentication.
    /// </summary>
    public class OpenIdConnectConfiguration
    {
        /// <summary>
        /// Gets or sets the OpenIDConnect Authority.
        /// </summary>
        public string? Authority { get; set; }

        /// <summary>
        /// Gets or sets the OpenIdConnect Client ID.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the OpenIDConnect Response types.
        /// </summary>
        public string? ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the OpenIDConnect Scopes.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets or sets the Callback URIs.
        /// </summary>
#pragma warning disable CA2227 //disable read-only Dictionary
        public Dictionary<string, Uri>? Callbacks { get; set; }

        /// <summary>
        /// Gets or sets the SaveTokens flag.
        /// </summary>
        public bool? SaveTokens { get; set; }

        /// <summary>
        /// Gets or sets whether to gather claims using the userinfo endpoint of the Auth Server.
        /// </summary>
        public bool? GetClaimsFromUserInfoEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the Uri for the signed out redirection.
        /// </summary>
#pragma warning disable CA1056 // Uri properties should not be strings
        public string? SignedOutRedirectUri { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

        /// <summary>
        /// Gets or sets whether Https Meta Data is required.
        /// </summary>
        public bool? RequireHttpsMetadata { get; set; }
    }
}
