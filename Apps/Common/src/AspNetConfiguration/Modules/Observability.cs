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
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using HealthGateway.Common.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Npgsql;
    using OpenTelemetry.Exporter;
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
        /// Configures http request logging.
        /// </summary>
        /// <param name="app">An app builder.</param>
        /// <returns>The app builder.</returns>
        public static IApplicationBuilder UseDefaultHttpRequestLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(
                opts =>
                {
                    opts.IncludeQueryInRequestPath = true;
                    opts.GetLevel = ExcludeHealthChecks;
                    opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
                    {
                        diagCtx.Set("User", httpCtx.User.Identity?.Name ?? string.Empty);
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
#pragma warning disable CA1506 - Avoid excessive class coupling
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
                            .AddOtlpExporter(
                                config =>
                                {
                                    config.Protocol = OtlpExportProtocol.HttpProtobuf;
                                    config.Endpoint = otlpConfig.Endpoint;
                                })
                            .AddSource(otlpConfig.ServiceName)
                            .SetSampler(new AlwaysOnSampler())
                            .SetResourceBuilder(
                                ResourceBuilder
                                    .CreateDefault()
                                    .AddService(serviceName: otlpConfig.ServiceName, serviceVersion: otlpConfig.ServiceVersion))
                            .AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation(
                                options => options.Filter = httpContext =>
                                    !otlpConfig.IgnorePathPrefixes.Any(s => httpContext.Request.Path.ToString().StartsWith(s, StringComparison.OrdinalIgnoreCase)))
                            .AddRedisInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation()
                            .AddNpgsql()
                            ;
                    })
                .WithMetrics(
                    builder =>
                    {
                        builder
                            .AddOtlpExporter(
                                config =>
                                {
                                    config.Protocol = otlpConfig.ExportProtocol;
                                    config.Endpoint = otlpConfig.Endpoint;
                                })
                            .AddHttpClientInstrumentation()
                            .AddAspNetCoreInstrumentation()
                            .AddRuntimeInstrumentation()
                            ;
                    })
                ;

            services.AddSingleton(TracerProvider.Default.GetTracer(otlpConfig.ServiceName));
            return services;
        }
#pragma warning restore CA1506

        private static LoggerConfiguration ConfigureDefaultLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, string serviceName)
        {
            loggerConfiguration
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("service", serviceName)
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithCorrelationId()
                .Enrich.WithCorrelationIdHeader()
                .Enrich.WithClientAgent()
                .Enrich.WithClientIp()
                .Enrich.WithSpan(new SpanOptions() { IncludeBaggage = true, IncludeTags = true, IncludeOperationName = true, IncludeTraceFlags = true })
                .WriteTo.Console(outputTemplate: LogOutputTemplate, formatProvider: CultureInfo.InvariantCulture)
                .ReadFrom.Configuration(configuration)
                ;

            return loggerConfiguration;
        }

        private static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double milliseconds, Exception? ex)
        {
            if (ex != null || ctx.Response.StatusCode >= (int)HttpStatusCode.InternalServerError)
            {
                return LogEventLevel.Error;
            }

            return ctx.Request.Path.StartsWithSegments("/health", StringComparison.InvariantCultureIgnoreCase)
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;
        }
    }
}
