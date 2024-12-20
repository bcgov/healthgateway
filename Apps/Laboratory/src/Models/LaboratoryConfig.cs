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
namespace HealthGateway.Laboratory.Models
{
    using System;

    /// <summary>
    /// Provides configuration data for the Laboratory Delegate.
    /// </summary>
    public class LaboratoryConfig
    {
        /// <summary>
        /// The name of the configuration section.
        /// </summary>
        public const string ConfigSectionKey = "Laboratory";

        /// <summary>
        /// Gets or sets the laboratory base endpoint.
        /// </summary>
        public Uri BaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the lab orders endpoint.
        /// </summary>
        public string LabOrdersEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Provincial Lab Information System lab endpoint.
        /// </summary>
        public string PlisLabEndPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public COVID-19 tests endpoint.
        /// </summary>
        public string PublicCovidTestsEndPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of records to retrieve in one call.
        /// </summary>
        public string FetchSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default time to wait for a new request.
        /// </summary>
        public int BackOffMilliseconds { get; set; }
    }
}
