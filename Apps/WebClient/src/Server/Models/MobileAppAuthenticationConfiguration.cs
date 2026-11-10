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
namespace HealthGateway.WebClient.Server.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Authentication settings used by the Health Gateway mobile app in place of the web client's.
    /// </summary>
    public class MobileAppAuthenticationConfiguration
    {
        /// <summary>
        /// Gets or sets the URI of the authentication server (Keycloak realm).
        /// </summary>
        public Uri? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the prefix of the client IDs the app uses.
        /// </summary>
        public string? ClientIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the identity providers the mobile app offers, in place of the web client's list.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Team decision")]
        public IdentityProviderConfiguration[] IdentityProviders { get; set; } = [];
    }
}
