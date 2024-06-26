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
namespace HealthGateway.Admin.Server.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Configuration data to be used by the Health Gateway Admin.
    /// </summary>
    public class AdminConfiguration
    {
        /// <summary>
        /// Gets or sets the enabled states of each toggleable feature.
        /// </summary>
        public Dictionary<string, bool> Features { get; set; } = [];

        /// <summary>
        /// Gets or sets the logging level used by the Admin.
        /// </summary>
        public string LogLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Admin timeout values.
        /// </summary>
        public TimeOutsConfiguration? Timeouts { get; set; }

        /// <summary>
        /// Gets or sets the ExternalUrls used by the Admin.
        /// </summary>
        public Dictionary<string, Uri> ExternalUrls { get; set; } = [];

        /// <summary>
        /// Gets or sets the unix timezone id.
        /// </summary>
        public string UnixTimeZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the windows timezone id.
        /// </summary>
        public string WindowsTimeZoneId { get; set; } = string.Empty;
    }
}
