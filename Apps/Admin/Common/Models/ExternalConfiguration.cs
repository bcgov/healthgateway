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
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A collection of configuration items for use by Health Gateway and
    /// approved applications.
    /// </summary>
    public class ExternalConfiguration
    {
        /// <summary>
        /// Gets or sets the client IP address.
        /// This value is populated at runtime with the client invoking the web service.
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// Gets or sets features enabled for the application.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, bool> Features { get; set; } = new();

        /// <summary>
        /// Gets or sets the OpenIdConnect configuration.
        /// </summary>
        public OpenIdConnectConfiguration OpenIdConnect { get; set; } = new();

        /// <summary>
        /// Gets or sets the Service Endpoints.
        /// </summary>
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, Uri> ServiceEndpoints { get; set; } = new();
    }
}
