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
    using System.Net;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

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

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddHttpClient<IHttpClientService, HttpClientService>();
            services.AddTransient<IHttpClientService, HttpClientService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHealthChecks()
                    .AddDbContextCheck<GatewayDbContext>();

            services
                .AddRazorPages()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="environment">The environment to use.</param>
        public static void UseHttp(IApplicationBuilder app, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable CORS
            string enableCors = configuration.GetValue<string>("AllowOrigins", string.Empty);
            if (!string.IsNullOrEmpty(enableCors))
            {
                app.UseCors(builder =>
                {
                    builder
                        .WithOrigins(enableCors)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            app.UseResponseCompression();

            // Setup response secure headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                await next().ConfigureAwait(true);
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
            app.UseEndpoints(routes =>
            {
                routes.MapControllers();
            });
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
            logger.LogInformation($"Forward Proxies enabled: {enabled}");
            if (enabled)
            {
                logger.LogDebug("Configuring Forward Headers");
                IPAddress[] proxyIPs = section.GetSection("KnownProxies").Get<IPAddress[]>() ?? Array.Empty<IPAddress>();
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.All;
                    options.RequireHeaderSymmetry = false;
                    options.ForwardLimit = null;
                    options.KnownNetworks.Clear();
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
            logger.LogInformation($"Forward Proxies enabled: {enabled}");
            if (enabled)
            {
                logger.LogDebug("Using Forward Headers");
                string basePath = section.GetValue<string>("BasePath");
                if (!string.IsNullOrEmpty(basePath))
                {
                    logger.LogInformation($"Forward BasePath is set to {basePath}, setting PathBase for app");
                    app.UsePathBase(basePath);
                    app.Use(async (context, next) =>
                    {
                        context.Request.PathBase = basePath;
                        await next.Invoke().ConfigureAwait(true);
                    });
                    app.UsePathBase(basePath);
                }

                logger.LogInformation("Enabling Use Forward Header");
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

            services.AddCors(options =>
            {
                options.AddPolicy("allowAny", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        /// <summary>
        /// Configures the app to to use content security policies.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void UseContentSecurityPolicy(IApplicationBuilder app, IConfiguration configuration)
        {
            ContentSecurityPolicyConfig cspConfig = new();
            configuration.GetSection("ContentSecurityPolicy").Bind(cspConfig);
            string csp = cspConfig.ContentSecurityPolicy();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", csp);
                await next().ConfigureAwait(true);
            });
        }

        /// <summary>
        /// Enables response caching and sets default no cache.
        /// </summary>
        /// <param name="app">The application build provider.</param>
        /// <param name="logger">The logger to use.</param>
        private static void UseResponseCaching(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Setting up Response Cache");
            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        NoCache = true,
                        NoStore = true,
                        MustRevalidate = true,
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Pragma] =
                    new string[] { "no-cache" };
                await next().ConfigureAwait(true);
            });
        }
    }
}
