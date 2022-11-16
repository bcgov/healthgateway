// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Models.CDogs
{
    /// <summary>
    /// The configuration for the Common Document Generation Service (CDOGS).
    /// </summary>
    public class CDogsConfig
    {
        /// <summary>
        /// Configuration section key for CDOGS.
        /// </summary>
        public const string CDogsConfigSectionKey = "CDOGS";

        /// <summary>
        /// Gets or sets the OpenShift service name.
        /// </summary>
        public string ServiceName { get; set; } = "HGCDOGS_SERVICE";

        /// <summary>
        /// Gets or sets the host suffix used to lookup the service host.
        /// </summary>
        public string ServiceHostSuffix { get; set; } = "_HOST";

        /// <summary>
        /// Gets or sets the port suffix used to lookup the service port.
        /// </summary>
        public string ServicePortSuffix { get; set; } = "_PORT";

        /// <summary>
        /// Gets or sets the base Url used to connect to the ODR Proxy.
        /// </summary>
        public string BaseEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether dynamic service lookup should occur.
        /// if enabled, the Url will be created using environment variables.
        /// If not enabled, the configured Url will be used.
        /// </summary>
        public bool DynamicServiceLookup { get; set; }
    }
}
