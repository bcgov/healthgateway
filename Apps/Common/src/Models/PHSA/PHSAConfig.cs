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
namespace HealthGateway.Common.Models.PHSA
{
    using System;

    /// <summary>
    /// Provides configuration data for the Immunization Delegate.
    /// </summary>
    public class PhsaConfig
    {
        /// <summary>
        /// The section key to use when binding this object.
        /// </summary>
        public const string ConfigurationSectionKey = "PHSA";

        /// <summary>
        /// Gets or sets the phsa base endpoint.
        /// </summary>
        public Uri BaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the total number of records to retrieve in one call.
        /// </summary>
        public int? FetchSize { get; set; }

        /// <summary>
        /// Gets or sets the default time to wait for a new request.
        /// </summary>
        public int BackOffMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries.
        /// </summary>
        public int MaxRetries { get; set; }
    }
}
