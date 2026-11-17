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
    using System.Collections.Generic;

    /// <summary>
    /// How long the client keeps what a service returned before asking again. Served as WebClient.Cache for every client,
    /// and as WebClient.Mobile.Cache for the mobile app's overrides. A service's own value overrides the default, and the
    /// client falls back to its embedded default when neither is set.
    /// </summary>
    public class CacheConfiguration
    {
        /// <summary>
        /// Gets or sets the cache duration in seconds for any service without its own value, or null when not set.
        /// </summary>
        public int? Default { get; set; }

        /// <summary>
        /// Gets or sets the cache duration in seconds for each service, keyed by service name (e.g. Communications), or null
        /// when none are set.
        /// </summary>
        public Dictionary<string, int>? Services { get; set; }
    }
}
