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
#pragma warning disable CA1303 //disable literal strings check
namespace HealthGateway.WebClient.Server
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Utils;
    using HealthGateway.WebClient.Server.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The environment variables provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureTracing(services);

            // Add Background Services
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            // Configure SPA
            services.AddControllersWithViews();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(options => options.RootPath = "ClientApp/dist");

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            // Add services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Deferred")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.startupConfig.UseContentSecurityPolicy(app);
            this.startupConfig.UsePermissionPolicy(app);
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);

            DisableTraceMethod(app);

            bool redirectToWww = this.configuration.GetSection("WebClient").GetValue<bool>("RedirectToWWW");
            if (redirectToWww)
            {
                RewriteOptions rewriteOption = new RewriteOptions()
                    .AddRedirectToWwwPermanent();
                app.UseRewriter(rewriteOption);
            }

            app.UseSpaStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles(
                new StaticFileOptions
                {
                    OnPrepareResponse = content =>
                    {
                        IHeaderDictionary headers = content.Context.Response.Headers;
                        StringValues contentType = headers["Content-Type"];
                        if (contentType != "application/x-gzip" && !content.File.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return;
                        }

                        FileExtensionContentTypeProvider mimeTypeProvider = new();
                        string fileNameToTry = content.File.Name.Substring(0, content.File.Name.Length - 3);
                        if (mimeTypeProvider.TryGetContentType(fileNameToTry, out string? mimeType))
                        {
                            headers.Append("Content-Encoding", "gzip");
                            headers.Append("Content-Type", mimeType);
                        }
                    },
                });

            this.startupConfig.UseHttp(app);
            this.startupConfig.EnrichTracing(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            });

            // Only run SPA fallback for real browser navigations (GET + Accept: text/html).
            // Prevents SPA middleware from trying to serve index.html for non-HTML requests
            // such as bots, scanners, and POST calls, which avoids unnecessary SPA errors.
            app.UseWhen(
                context =>
                {
                    return HttpMethods.IsGet(context.Request.Method)
                           && context.Request.Headers.TryGetValue(HeaderNames.Accept, out StringValues accept)
                           && accept.Any(v => v?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);
                },
                spaApp => { spaApp.UseSpa(spa => { spa.Options.SourcePath = "ClientApp"; }); });
        }

        private static void DisableTraceMethod(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "TRACE")
                {
                    context.Response.StatusCode = 405;
                    return;
                }

                await next.Invoke();
            });
        }
    }
}
