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

    /// <summary>
    /// A collection of configuration items for use by Health Gateway Mobile.
    /// </summary>
    public class MobileConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether the mobile application should be considered online or not.
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// Gets or sets the base url for the endpoints to be used by the mobile application.
        /// </summary>
        public Uri BaseUrl { get; set; }
    }
}
