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
    using System.Globalization;
    using System.Net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection.XmlEncryption;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Npgsql.Replication.PgOutput.Messages;
    using Serilog;
    using Serilog.Enrichers.Span;
    using Serilog.Events;
    using Serilog.Exceptions;

    /// <summary>
    /// Methods to configure observability dependencies and settings
    /// </summary>
    public static class Observability
    {
        /// <summary>
        /// Log output format template
        /// </summary>
        public const string LogOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";

        public static IHostBuilder UseDefaultLogging(this IHostBuilder builder, string serviceName)
        {
            builder.UseSerilog((ctx, services, config) => config.ConfigureDefaultLogging(ctx.Configuration, services, serviceName));

            return builder;
        }


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

        private static LoggerConfiguration ConfigureDefaultLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, IServiceProvider services, string serviceName)
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
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
                .Enrich.WithSpan()
                .WriteTo.Console(outputTemplate: LogOutputTemplate, formatProvider: CultureInfo.InvariantCulture)
                ;

            return loggerConfiguration;
        }

        private static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception? ex)
        {
            if (ex != null || ctx.Response.StatusCode >= (int)HttpStatusCode.InternalServerError)
            {
                return LogEventLevel.Error;
            }

            return ctx.Request.Path.StartsWithSegments("/hc", StringComparison.InvariantCultureIgnoreCase)
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;
        }

        // public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string appName)
        // {
        //services.AddOpenTelemetryTracing(builder =>
        //{
        //    builder
        //        .AddConsoleExporter()
        //        .AddSource(appName)
        //        .SetResourceBuilder(ResourceBuilder
        //            .CreateDefault()
        //            .AddService(serviceName: appName, serviceVersion: Environment.GetEnvironmentVariable("VERSION")))
        //        .AddHttpClientInstrumentation()
        //        .AddAspNetCoreInstrumentation()
        //        .AddGrpcCoreInstrumentation()
        //        .AddGrpcClientInstrumentation()
        //        .AddRedisInstrumentation()
        //        .AddConsoleExporter();
        //});

        //services.AddSingleton(TracerProvider.Default.GetTracer(appName));
        //
        // var listener = new SerilogTraceListener.SerilogTraceListener();
        // Trace.Listeners.Add(listener);
        //
        // return services;
    }
}
