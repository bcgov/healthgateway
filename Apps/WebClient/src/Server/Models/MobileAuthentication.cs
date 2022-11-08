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
namespace HealthGateway.WebClient.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A collection of authentication configuration items for use by Health Gateway Mobile.
    /// </summary>
    public class MobileAuthentication
    {
        /// <summary>
        /// Gets or sets the Uri for the Authentication server.
        /// </summary>
        public Uri? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Identity Provider to be used.
        /// </summary>
        public string? IdentityProviderId { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// </summary>
        [SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Special URI Values")]
        public string? RedirectUri { get; set; }
    }
}
