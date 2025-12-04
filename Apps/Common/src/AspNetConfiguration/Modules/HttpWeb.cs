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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using HealthGateway.Common.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Provides ASP.Net Services related to Http.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class HttpWeb
    {
        /// <summary>
        /// Configures the http services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void ConfigureHttpServices(IServiceCollection services, ILogger logger)
        {
            logger.LogDebug("Configure Http Services...");

            services.AddResponseCompression(options => options.EnableForHttps = true);

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHealthChecks();

            services
                .AddRazorPages()
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="blazor">If true, will add blazor optimizations for static files.</param>
        /// <param name="useExceptionPage">
        /// If true, app will use development exception page. Should be false when using problem
        /// details middleware.
        /// </param>
        public static void UseHttp(IApplicationBuilder app, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment, bool blazor, bool useExceptionPage)
        {
            if (!environment.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            if (useExceptionPage && environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (!blazor)
            {
                app.UseStaticFiles();
            }
            else
            {
                app.UseStaticFiles(GetBlazorStaticFileOptions(environment));
            }

            RequestLoggingSettings requestLoggingSettings = new();
            configuration.GetSection("RequestLogging").Bind(requestLoggingSettings);
            if (requestLoggingSettings.Enabled)
            {
                app.UseDefaultHttpRequestLogging(requestLoggingSettings.ExcludedPaths?.ToArray());
            }

            app.UseRouting();

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable CORS
            string? enableCors = configuration.GetValue<string>("AllowOrigins");
            if (!string.IsNullOrEmpty(enableCors))
            {
                app.UseCors(
                    builder =>
                    {
                        builder
                            .WithOrigins(enableCors)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            }

            // Setup response secure headers
            app.Use(
                async (context, next) =>
                {
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");
                    await next();
                });

            // Enable Cache control and set defaults
            UseResponseCaching(app, logger);
        }

        /// <summary>
        /// Configures the app to use Rest services.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void UseRest(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Use Rest...");
            app.UseEndpoints(routes => routes.MapControllers());
        }

        /// <summary>
        /// Configures Forward proxies.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureForwardHeaders(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("ForwardProxies");
            bool enabled = section.GetValue<bool>("Enabled");
            logger.LogInformation("Forward proxies enabled: {ProxiesEnabled}", enabled);
            if (enabled)
            {
                logger.LogDebug("Configuring forwarded headers");
                IPAddress[] proxyIPs = section.GetSection("KnownProxies").Get<IPAddress[]>() ?? [];
                services.Configure<ForwardedHeadersOptions>(
                    options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.All;
                        options.RequireHeaderSymmetry = false;
                        options.ForwardLimit = null;
                        options.KnownIPNetworks.Clear();
                        options.KnownProxies.Clear();
                        foreach (IPAddress ip in proxyIPs)
                        {
                            options.KnownProxies.Add(ip);
                        }
                    });
            }
        }

        /// <summary>
        /// Configures the app to use x-forwarded-for headers to obtain the real client IP.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void UseForwardHeaders(IApplicationBuilder app, ILogger logger, IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("ForwardProxies");
            bool enabled = section.GetValue<bool>("Enabled");
            logger.LogInformation("Forward proxies enabled: {ProxiesEnabled}", enabled);
            if (enabled)
            {
                string basePath = section.GetValue<string>("BasePath") ?? string.Empty;
                if (!string.IsNullOrEmpty(basePath))
                {
                    logger.LogInformation("Setting PathBase for app to {BasePath}", basePath);
                    app.UsePathBase(basePath);
                    app.Use(
                        async (context, next) =>
                        {
                            context.Request.PathBase = basePath;
                            await next.Invoke();
                        });
                    app.UsePathBase(basePath);
                }

                logger.LogInformation("Enabling Use Forwarded Headers");
                app.UseForwardedHeaders();
            }
        }

        /// <summary>
        /// Configures Access control that allows any origin, header and method.
        /// </summary>
        /// <param name="services">The service collection to add forward proxies into.</param>
        /// <param name="logger">The logger to use.</param>
        public static void ConfigureAccessControl(IServiceCollection services, ILogger logger)
        {
            logger.LogDebug("Configure Access Control...");

            services.AddCors(options => options.AddPolicy("allowAny", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        }

        /// <summary>
        /// Configures the app to use content security policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void UseContentSecurityPolicy(IApplicationBuilder app, IConfiguration configuration)
        {
            ContentSecurityPolicyConfig cspConfig = new();
            configuration.GetSection("ContentSecurityPolicy").Bind(cspConfig);
            string csp = cspConfig.ContentSecurityPolicy();
            app.Use(
                async (context, next) =>
                {
                    context.Response.Headers.Append("Content-Security-Policy", csp);
                    await next();
                });
        }

        /// <summary>
        /// Configures the app to use permission policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void UsePermissionPolicy(IApplicationBuilder app, IConfiguration configuration)
        {
            string? policyValue = configuration["PermissionPolicy"];
            if (policyValue != null)
            {
                app.Use(
                    async (context, next) =>
                    {
                        context.Response.Headers.Append("Permissions-Policy", policyValue);
                        await next();
                    });
            }
        }

        /// <summary>
        /// Enables response caching and sets default no cache.
        /// </summary>
        /// <param name="app">The application build provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void UseResponseCaching(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Setting up Response Cache");
            app.UseResponseCaching();

            app.Use(
                async (context, next) =>
                {
                    context.Response.GetTypedHeaders().CacheControl =
                        new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            MustRevalidate = true,
                        };
                    context.Response.Headers[HeaderNames.Pragma] = new StringValues("no-cache");
                    await next();
                });
        }

        private static StaticFileOptions GetBlazorStaticFileOptions(IWebHostEnvironment environment)
        {
            return new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    if (context.File.Name == "service-worker-assets.js")
                    {
                        context.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Append("Expires", "-1");
                    }

                    if (context.File.Name == "blazor.boot.json")
                    {
                        context.Context.Response.Headers.Remove("blazor-environment");
                        context.Context.Response.Headers.Append("blazor-environment", environment.EnvironmentName);
                    }
                },
            };
        }
    }

    /// <summary>
    /// Settings to control request logging.
    /// </summary>
    /// <param name="Enabled">Enable or disable request logging.</param>
    /// <param name="ExcludedPaths">Optional request paths to exclude, can handle * wildcard in prefix or postfix.</param>
    [ExcludeFromCodeCoverage]
    public record RequestLoggingSettings(bool Enabled = true, IEnumerable<string>? ExcludedPaths = null);
}
