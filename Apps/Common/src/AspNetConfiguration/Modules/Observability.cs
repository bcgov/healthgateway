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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using Azure.Monitor.OpenTelemetry.Exporter;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Npgsql;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using Serilog;
    using Serilog.Enrichers.Span;
    using Serilog.Events;
    using Serilog.Exceptions;

    /// <summary>
    /// Methods to configure observability dependencies and settings
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Observability
    {
        /// <summary>
        /// Log output format template
        /// </summary>
        public const string LogOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Configures logging with default settings.
        /// </summary>
        /// <param name="builder">A host builder.</param>
        /// <param name="serviceName">The service name.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder UseDefaultLogging(this IHostBuilder builder, string? serviceName = null)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = Assembly.GetEntryAssembly()!.GetName().Name!;
            }

            builder.UseSerilog((ctx, config) => config.ConfigureDefaultLogging(ctx.Configuration, serviceName));

            return builder;
        }

        /// <summary>
        /// Configures http request logging
        /// </summary>
        /// <param name="app">An app builder</param>
        /// <param name="excludePaths">Path to exclude - can use wildcards * for prefix or postfix</param>
        /// <returns>The app builder</returns>
        public static IApplicationBuilder UseDefaultHttpRequestLogging(this IApplicationBuilder app, string[]? excludePaths = null)
        {
            app.UseSerilogRequestLogging(
                opts =>
                {
                    opts.IncludeQueryInRequestPath = true;
                    opts.GetLevel = (httpCtx, _, exception) => ExcludePaths(httpCtx, exception, excludePaths ?? []);
                    opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
                    {
                        diagCtx.Set("Host", httpCtx.Request.Host.Value);
                        diagCtx.Set("ContentLength", httpCtx.Response.ContentLength?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                        diagCtx.Set("Protocol", httpCtx.Request.Protocol);
                        diagCtx.Set("Scheme", httpCtx.Request.Scheme);
                    };
                });

            return app;
        }

        /// <summary>
        /// Adds OpenTelemetry components to DI.
        /// </summary>
        /// <param name="services">A DI container.</param>
        /// <param name="otlpConfig">OpenTelemetry configuration values.</param>
        /// <returns>The DI container.</returns>
#pragma warning disable CA1506 //Avoid excessive class coupling
        public static IServiceCollection AddOpenTelemetryDefaults(this IServiceCollection services, OpenTelemetryConfig otlpConfig)
        {
            if (string.IsNullOrEmpty(otlpConfig.ServiceName))
            {
                otlpConfig.ServiceName = Assembly.GetEntryAssembly()!.GetName().Name;
            }

            if (string.IsNullOrEmpty(otlpConfig.ServiceVersion))
            {
                otlpConfig.ServiceVersion = Environment.GetEnvironmentVariable("VERSION");
            }

            services.AddOpenTelemetry()
                .WithTracing(
                    builder =>
                    {
                        builder
                            .AddSource(otlpConfig.ServiceName)
                            .SetSampler(new AlwaysOnSampler())
                            .SetResourceBuilder(
                                ResourceBuilder
                                    .CreateDefault()
                                    .AddService(otlpConfig.ServiceName, serviceVersion: otlpConfig.ServiceVersion))
                            .AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation(
                                options =>
                                {
                                    options.Filter = httpContext => !Array.Exists(
                                        otlpConfig.IgnorePathPrefixes,
                                        s => httpContext.Request.Path.ToString().StartsWith(s, StringComparison.OrdinalIgnoreCase));
                                })
                            .AddRedisInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation()
                            .AddNpgsql()
                            ;

                        if (otlpConfig.TraceConsoleExporterEnabled)
                        {
                            builder.AddConsoleExporter();
                        }

                        if (!string.IsNullOrEmpty(otlpConfig.AzureConnectionString))
                        {
                            builder.AddAzureMonitorTraceExporter(options => options.ConnectionString = otlpConfig.AzureConnectionString);
                        }

                        if (otlpConfig.Endpoint != null)
                        {
                            builder.AddOtlpExporter(
                                config =>
                                {
                                    config.Protocol = otlpConfig.ExportProtocol;
                                    config.Endpoint = otlpConfig.Endpoint;
                                });
                        }
                    })
                .WithMetrics(
                    builder =>
                    {
                        builder
                            .AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation()
                            .AddRuntimeInstrumentation()
                            ;

                        if (otlpConfig.MetricsConsoleExporterEnabled)
                        {
                            builder.AddConsoleExporter();
                        }

                        if (!string.IsNullOrEmpty(otlpConfig.AzureConnectionString))
                        {
                            builder.AddAzureMonitorMetricExporter(options => options.ConnectionString = otlpConfig.AzureConnectionString);
                        }

                        if (otlpConfig.Endpoint != null)
                        {
                            builder.AddOtlpExporter(
                                config =>
                                {
                                    config.Protocol = otlpConfig.ExportProtocol;
                                    config.Endpoint = otlpConfig.Endpoint;
                                });
                        }
                    })
                ;

            services.AddSingleton(TracerProvider.Default.GetTracer(otlpConfig.ServiceName));
            return services;
        }
#pragma warning restore CA1506

        /// <summary>
        /// Adds middleware to enrich tracing telemetry with additional properties.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void EnrichTracing(IApplicationBuilder app, IConfiguration configuration)
        {
            OpenTelemetryConfig openTelemetryConfig = new();
            configuration.GetSection("OpenTelemetry").Bind(openTelemetryConfig);

            if (openTelemetryConfig.Enabled)
            {
                app.Use(
                    async (context, next) =>
                    {
                        string subject = GetRequestHdid(context);
                        EnrichActivityWithBaggage("Subject", subject, Activity.Current);

                        string user = context.User.Identity?.Name ?? string.Empty;
                        EnrichActivityWithBaggage("User", user, Activity.Current);

                        await next();
                    });
            }
        }

        private static string GetRequestHdid(HttpContext context)
        {
            return HttpContextHelper.GetResourceHdid(context, FhirSubjectLookupMethod.Route)
                   ?? HttpContextHelper.GetResourceHdid(context, FhirSubjectLookupMethod.Parameter)
                   ?? string.Empty;
        }

        private static void EnrichActivityWithBaggage(string key, string value, Activity? activity)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            activity?.AddBaggage(key, value);
        }

        private static void ConfigureDefaultLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, string serviceName)
        {
            loggerConfiguration
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Application", serviceName)
                .Enrich.WithEnvironmentName()
                .Enrich.WithCorrelationId()
                .Enrich.WithCorrelationIdHeader()
                .Enrich.WithRequestHeader("User-Agent")
                .Enrich.WithClientIp()
                .Enrich.WithSpan(new SpanOptions { IncludeBaggage = true, IncludeTags = true, IncludeOperationName = true, IncludeTraceFlags = true })
                .ReadFrom.Configuration(configuration);
        }

        private static LogEventLevel ExcludePaths(HttpContext ctx, Exception? ex, string[] excludedPaths)
        {
            if (ex != null || ctx.Response.StatusCode >= (int)HttpStatusCode.InternalServerError)
            {
                return LogEventLevel.Error;
            }

            return Array.Exists(excludedPaths, path => IsWildcardMatch(ctx.Request.Path, path))
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;
        }

        private static bool IsWildcardMatch(PathString requestPath, string path)
        {
            if (!requestPath.HasValue)
            {
                return false;
            }

            if (path.EndsWith('*'))
            {
                return requestPath.Value!.StartsWith(path.Replace("*", string.Empty, StringComparison.InvariantCultureIgnoreCase), StringComparison.InvariantCultureIgnoreCase);
            }

            if (path.StartsWith('*'))
            {
                return requestPath.Value!.EndsWith(path.Replace("*", string.Empty, StringComparison.InvariantCultureIgnoreCase), StringComparison.InvariantCultureIgnoreCase);
            }

            return requestPath.Equals(path, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
