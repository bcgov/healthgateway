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

    /// <summary>
    /// A collection of configuration items used to take Health Gateway offline.
    /// If the OfflineModeConfiguration is null, then offline mode is disabled.
    /// </summary>
    public class OfflineModeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineModeConfiguration"/> class.
        /// </summary>
        public OfflineModeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OfflineModeConfiguration"/> class.
        /// </summary>
        /// <param name="whitelist">The initialied list of whitelisted ip addresses.</param>
        public OfflineModeConfiguration(IList<string> whitelist)
        {
            this.Whitelist = whitelist;
        }

        /// <summary>
        /// Gets or sets the beginning datetime for the offline mode.
        /// </summary>
        public DateTime StartDateTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the ending datetime for the offline mode.
        /// A null value means that we are permanently in offline mode.
        /// If the value is in the past then offline mode is disabled.
        /// </summary>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the offline message to be displayed to the client if offline mode is enabled.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of IPs that can use the webclient during an outage.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public IList<string> Whitelist { get; set; } = new List<string>();
    }
}
