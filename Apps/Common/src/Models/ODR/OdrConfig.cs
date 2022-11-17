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
namespace HealthGateway.Common.Models.ODR
{
    /// <summary>
    /// The configuration for the ODR portion of the ResetMedStatement delegate.
    /// </summary>
    public class OdrConfig
    {
        /// <summary>
        /// The section key to use when binding this object.
        /// </summary>
        public const string OdrConfigSectionKey = "ODR";

        /// <summary>
        /// Gets or sets the OpenShift service name.
        /// </summary>
        public string ServiceName { get; set; } = "ODRPROXY_SERVICE";

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
        /// Gets or sets the endpoint path for the patient profile service.
        /// </summary>
        public string PatientProfileEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint path for the msp visits service.
        /// </summary>
        public string MspVisitsEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint path for the protective word service.
        /// </summary>
        public string ProtectiveWordEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time to live for cache objects.
        /// </summary>
        public int CacheTtl { get; set; } = 1440;

        /// <summary>
        /// Gets or sets a value indicating whether dynamic service lookup should occur.
        /// if enabled, the Url will be created using environment variables.
        /// If not enabled, the configured Url will be used.
        /// </summary>
        public bool DynamicServiceLookup { get; set; }
    }
}
