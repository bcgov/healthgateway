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
    /// A collection of configuration items for use by Health Gateway and
    /// approved applications.
    /// </summary>
    public class ExternalConfiguration
    {
        /// <summary>
        /// Gets or sets the OpenIdConnect configuration.
        /// </summary>
        public OpenIdConnectConfiguration OpenIdConnect { get; set; } = new OpenIdConnectConfiguration();

        /// <summary>
        /// Gets or sets the List of Identity providers.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Team decision")]
        public IdentityProviderConfiguration[] IdentityProviders { get; set; } = Array.Empty<IdentityProviderConfiguration>();

        /// <summary>
        /// Gets or sets the Health Gateway Webclient specific configuration.
        /// </summary>
        public WebClientConfiguration WebClient { get; set; } = new WebClientConfiguration();

        /// <summary>
        /// Gets or sets the Service Endpoints.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, System.Uri> ServiceEndpoints { get; set; } = new Dictionary<string, Uri>();
    }
}
