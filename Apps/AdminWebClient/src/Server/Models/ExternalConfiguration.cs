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
namespace HealthGateway.Admin.Models
{
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
        /// Gets or sets the Health Gateway Webclient specific configuration.
        /// </summary>
        public AdminConfiguration Admin { get; set; } = new AdminConfiguration();

        /// <summary>
        /// Gets or sets the forward proxies configuration.
        /// </summary>
        public ForwardProxiesConfiguration ForwardProxies { get; set; } = new ForwardProxiesConfiguration();
    }
}
