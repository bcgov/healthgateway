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
    using OpenTelemetry.Exporter;

    /// <summary>
    /// Provides configuration data for the Notifications Settings PHSA API.
    /// </summary>
    public class OpenTelemetryConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether Open Telemetry is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the name to use for the service.
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the version to use for the service.
        /// </summary>
        public string? ServiceVersion { get; set; }

        /// <summary>
        /// Gets or sets the path prefixes that tracing will ignore.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Configuration Binding")]
        public string[] IgnorePathPrefixes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets OpenTelemetry collector endpoint.
        /// </summary>
        public Uri? Endpoint { get; set; }

        /// <summary>
        /// Gets or sets OpenTelemetry export protocol.
        /// </summary>
        public OtlpExportProtocol ExportProtocol { get; set; } = OtlpExportProtocol.HttpProtobuf;

        /// <summary>
        /// Gets or sets a value indicating whether  OpenTelemetry console export for tracing, default to false.
        /// </summary>
        public bool TraceConsoleExporterEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether OpenTelemetry console export for metrics, default to false.
        /// </summary>
        public bool MetricsConsoleExporterEnabled { get; set; }
    }
}
