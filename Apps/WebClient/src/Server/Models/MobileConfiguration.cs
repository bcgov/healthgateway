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
namespace HealthGateway.WebClient.Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Configuration data to be used by the Health Gateway Mobile App.
    /// </summary>
    /// <param name="Online">Gets a value indicating whether the mobile application should be considered online.</param>
    /// <param name="BaseUrl">Gets the base URL for the endpoints used by the mobile application.</param>
    /// <param name="Authentication">Gets settings for authentication.</param>
    /// <param name="Version">Gets the version number of this configuration.</param>
    public record MobileConfiguration(
        bool Online = false,
        Uri? BaseUrl = null,
        MobileAuthenticationSettings? Authentication = null,
        int Version = 0)
    {
        /// <summary>
        /// Gets or sets the collection of enabled datasets.
        /// </summary>
        public IEnumerable<string> Datasets { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the collection of enabled datasets for dependents.
        /// </summary>
        public IEnumerable<string> DependentDatasets { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the collection of enabled services.
        /// </summary>
        public IEnumerable<string> Services { get; set; } = Enumerable.Empty<string>();
    }
}
