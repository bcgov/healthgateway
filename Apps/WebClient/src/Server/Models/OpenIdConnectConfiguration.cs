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
        /// Gets or sets the OpenIDConnect Authority Uri.
        /// </summary>
        public Uri? Uri { get; set; }

        /// <summary>
        /// Gets or sets the OpenIdConnect Client ID.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the OpenIdConnect Realms.
        /// </summary>
        public string Realms { get; set; } = string.Empty;
    }
}
