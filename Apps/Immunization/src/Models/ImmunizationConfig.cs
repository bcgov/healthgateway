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
namespace HealthGateway.Immunization.Models
{
    /// <summary>
    /// Provides configuration data for the Immunization Delegate.
    /// </summary>
    public class PHSAConfig
    {
        /// <summary>
        /// Gets or sets the phsa base endpoint.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the immunization endpoint.
        /// </summary>
        public string ImmunizationEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vaccine status endpoint.
        /// </summary>
        public string VaccineStatusEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of records to retrieve in one call.
        /// </summary>
        public string FetchSize { get; set; } = string.Empty;
    }
}
