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
namespace HealthGateway.Common.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides configuration data for the Notifications Settings PHSA API.
    /// </summary>
    public class OpenTelemetryConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether Open Telemetry is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name to use for the service.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the set of sources to listen for activities on.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Configuration Binding")]
        public string[] Sources { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether tracing should be dumped to the console.
        /// </summary>
        public bool ConsoleEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tracing to zipkin should be enabled.
        /// </summary>
        public bool ZipkinEnabled { get; set; }

        /// <summary>
        /// Gets or sets the URI to send Zipkin tracing results.
        /// </summary>
        public Uri? ZipkinUri { get; set; }

        /// <summary>
        /// Gets or sets the path prefixes that tracing will ignore.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Configuration Binding")]
        public string[] IgnorePathPrefixes { get; set; } = Array.Empty<string>();
    }
}
