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
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    /// <summary>
    /// Privies Utility methods for common modules.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Utility
    {
        /// <summary>
        /// Configures OpenTelemetry tracing.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureTracing(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("Setting up OpenTelemetry");
            OpenTelemetryConfig config = new();
            configuration.GetSection("OpenTelemetry").Bind(config);
            if (config.Enabled)
            {
                services.AddOpenTelemetryTracing(
                    builder =>
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(config.ServiceName))
                            .AddAspNetCoreInstrumentation(
                                options => options.Filter = httpContext => !config.IgnorePathPrefixes.Any(s => httpContext.Request.Path.ToString().StartsWith(s, StringComparison.OrdinalIgnoreCase)))
                            .AddHttpClientInstrumentation()
                            .AddNpgsql()
                            .AddSource(config.Sources);
                        if (config.ZipkinEnabled)
                        {
                            builder.AddZipkinExporter(options => options.Endpoint = config.ZipkinUri);
                        }

                        if (config.ConsoleEnabled)
                        {
                            builder.AddConsoleExporter();
                        }
                    });
            }
        }

        /// <summary>
        /// Fetches the base path from the configuration.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>The BasePath config for the ForwardProxies.</returns>
        public static string GetAppBasePath(ILogger logger, IConfiguration configuration)
        {
            string basePath = string.Empty;
            IConfigurationSection section = configuration.GetSection("ForwardProxies");
            if (section.GetValue("Enabled", false))
            {
                basePath = section.GetValue<string>("BasePath");
            }

            logger.LogDebug($"BasePath = {basePath}");
            return basePath;
        }
    }
}
