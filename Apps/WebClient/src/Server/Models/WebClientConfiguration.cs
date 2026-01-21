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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Configuration data to be used by the Health Gateway Webclient.
    /// </summary>
    public class WebClientConfiguration
    {
        /// <summary>
        /// Gets or sets the value for web client configuration section key.
        /// </summary>
        public const string ConfigurationSectionKey = "WebClient";

        /// <summary>
        /// Gets or sets the logging level used by the Webclient.
        /// </summary>
        public string LogLevel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Webclient timeout values.
        /// </summary>
        public TimeOutsConfiguration Timeouts { get; set; } = new();

        /// <summary>
        /// Gets or sets the URL for the Access My Health portal.
        /// </summary>
        public Uri? AccessMyHealthUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the beta application.
        /// </summary>
        public Uri? BetaUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the classic application.
        /// </summary>
        public Uri? ClassicUrl { get; set; }

        /// <summary>
        /// Gets or sets the ExternalURLs used by the Webclient.
        /// </summary>
        [JsonPropertyName("externalURLs")]
        public Dictionary<string, Uri> ExternalUrLs { get; set; } = [];

        /// <summary>
        /// Gets or sets the FeatureToggleFilePath.
        /// </summary>
        public string FeatureToggleFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state of the Health Gateway features.
        /// </summary>
        public FeatureToggleConfiguration? FeatureToggleConfiguration { get; set; }

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
        /// Gets or sets the email verification's expiry seconds.
        /// </summary>
        public int EmailVerificationExpirySeconds { get; set; }

        /// <summary>
        /// Gets or sets the offline mode configuration.
        /// </summary>
        public OfflineModeConfiguration? OfflineMode { get; set; }

        /// <summary>
        /// Gets or sets the client IP address.
        /// This value is populated at runtime with the client invoking the web service.
        /// </summary>
        [JsonPropertyName("clientIP")]
        public string? ClientIp { get; set; }

        /// <summary>
        /// Gets or sets the robots file path.
        /// </summary>
        public string RobotsFilePath { get; set; } = string.Empty;
    }
}
