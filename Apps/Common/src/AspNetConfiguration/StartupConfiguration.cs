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
namespace HealthGateway.Common.AspNetConfiguration
{
    using System;
    using System.IO;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Swagger;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.AspNetCore.SpaServices.Webpack;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The startup configuration class.
    /// </summary>
    public class StartupConfiguration
    {
        private readonly IHostingEnvironment environment;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupConfiguration"/> class.
        /// </summary>
        /// <param name="env">The environment variables provider.</param>
        /// <param name="config">The configuration provider.</param>
        /// <param name="logger">The logger provider.</param>
        public StartupConfiguration(IConfiguration config, IHostingEnvironment env, ILogger logger)
        {
            this.environment = env;
            this.configuration = config;
            this.logger = logger;
        }

        /// <summary>
        /// Configures the http services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureHttpServices(IServiceCollection services)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("Configure Http Services...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            services.AddHttpClient();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            // Inject HttpContextAccessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHealthChecks();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });
        }

        public void ConfigureAuthorizationServices(IServiceCollection services)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("ConfigureAuthoirzationServices...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            services.AddAuthorization(options =>
                {
                    options.AddPolicy("ReadPolicy", policy =>
                        policy.Requirements.Add(new UserIsPatientRequirement()));
                });
            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureAuthServicesForJwtBearer(IServiceCollection services)
        {
            bool debugEnabled = this.environment.IsDevelopment() || this.configuration.GetValue<bool>("EnableDebug", true);
            this.logger.LogDebug($"Debug configuration is ${debugEnabled}");

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                this.configuration.GetSection("OpenIdConnect").Bind(o);
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        return c.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = "AuthenticationFailed",
                            Message = c.Exception.ToString(),
                        }));
                    },
                };
            });
        }

        /// <summary>
        /// Configures the swagger services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        public void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.Configure<SwaggerSettings>(this.configuration.GetSection(nameof(SwaggerSettings)));

            services
                .AddApiVersionWithExplorer()
                .AddSwaggerOptions()
                .AddSwaggerGen();
        }

        /// <summary>
        /// Configures the app to use auth.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseAuth(IApplicationBuilder app)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("Use Auth...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            // Enable jwt authentication
            app.UseAuthentication();
        }

        /// <summary>
        /// Configures the app to use web client.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseWebClient(IApplicationBuilder app)
        {
            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp"),
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = (content) =>
                {
                    var headers = content.Context.Response.Headers;
                    var contentType = headers["Content-Type"];
                    if (contentType != "application/x-gzip" && !content.File.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return;
                    }

                    var mimeTypeProvider = new FileExtensionContentTypeProvider();
                    var fileNameToTry = content.File.Name.Substring(0, content.File.Name.Length - 3);
                    if (mimeTypeProvider.TryGetContentType(fileNameToTry, out var mimeType))
                    {
                        headers.Add("Content-Encoding", "gzip");
                        headers["Content-Type"] = mimeType;
                    }
                },
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        /// <summary>
        /// Configures the app to use http.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseHttp(IApplicationBuilder app)
        {
            if (this.environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable CORS
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(this.configuration.GetValue<string>("AllowOrigins", "*"))
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // forwarded Header middleware required for apps behind proxies and load balancers
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            });

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        /// <summary>
        /// Configures the app to use swagger.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseSwagger(IApplicationBuilder app)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("Use Swagger...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            app.UseSwaggerDocuments();
        }
    }
}
