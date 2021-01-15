﻿//-------------------------------------------------------------------------
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
    using System.Collections.Generic;

    /// <summary>
    /// Configuration data to be used by the Health Gateway Webclient.
    /// </summary>
    public class WebClientConfiguration
    {
        /// <summary>
        /// Gets or sets the logging level used by the Webclient.
        /// </summary>
        public string LogLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Webclient timeout values.
        /// </summary>
        public TimeOutsConfiguration Timeouts { get; set; } = new TimeOutsConfiguration();

        /// <summary>
        /// Gets or sets the Webclient registration status.
        /// </summary>
        public string RegistrationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ExternalURLs used by the Webclient.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, System.Uri> ExternalURLs { get; set; } = new Dictionary<string, System.Uri>();

        /// <summary>
        /// Gets or sets the state for each of our modules.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public Dictionary<string, bool> Modules { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets or sets the number of hours until an account is removed after being closed.
        /// </summary>
        public int HoursForDeletion { get; set; }

        /// <summary>
        /// Gets or sets the minimum required patient age allowed for registration.
        /// </summary>
        public int? MinPatientAge { get; set; }

        /// <summary>
        /// Gets or sets the maximum age of a dependent.
        /// </summary>
        public int? MaxDependentAge { get; set; }

        /// <summary>
        /// Gets or sets the offline mode configuration.
        /// </summary>
        public OfflineModeConfiguration? OfflineMode { get; set; }

        /// <summary>
        /// Gets or sets the client IP address.
        /// This value is populated at runtime with the client invoking the web service.
        /// </summary>
        public string? ClientIP { get; set; }
    }
}
