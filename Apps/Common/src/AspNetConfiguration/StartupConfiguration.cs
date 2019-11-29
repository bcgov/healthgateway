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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Swagger;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
#pragma warning disable CA1303 // Do not pass literals as localized parameters

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
            this.logger.LogDebug("Configure Http Services...");

            services.AddHttpClient();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHealthChecks();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });
        }

        /// <summary>
        /// Configures the authorization services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuthorizationServices(IServiceCollection services)
        {
            this.logger.LogDebug("ConfigureAuthorizationServices...");

            // Adding claims check to ensure that user has an hdid as part of its claim
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNameConstants.PatientOnly, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("hdid");
                });
                options.AddPolicy(PolicyNameConstants.UserIsPatient, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new UserIsPatientRequirement());
                });
            });

            // Configuration Service
            services.AddScoped<IAuthorizationHandler, UserAuthorizationHandler>();
        }

        /// <summary>
        /// Configures the auth services for json web token bearer.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureAuthServicesForJwtBearer(IServiceCollection services)
        {
            IAuditLogger auditLogger = services.BuildServiceProvider().GetService<IAuditLogger>();
            bool debugEnabled = this.environment.IsDevelopment() || this.configuration.GetValue<bool>("EnableDebug", true);
            this.logger.LogDebug($"Debug configuration is {debugEnabled}");

            // Displays sensitive data from the jwt if the environment is development only
            IdentityModelEventSource.ShowPII = debugEnabled;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.RequireHttpsMetadata = true;
                o.IncludeErrorDetails = true;
                this.configuration.GetSection("OpenIdConnect").Bind(o);

                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                };
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = (ctx) => { return this.OnAuthenticationFailed(ctx, auditLogger); },
                };
            });
        }

        /// <summary>
        /// Configures the audit services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        public void ConfigureAuditServices(IServiceCollection services)
        {
            this.logger.LogDebug("ConfigureAuditServices...");

            services.AddMvc(options => options.Filters.Add(typeof(AuditFilter)));
            services.AddDbContextPool<GatewayDbContext>(options => options.UseNpgsql(
                    this.configuration.GetConnectionString("GatewayConnection")));
            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddTransient<IWriteAuditEventDelegate, DBWriteAuditEventDelegate>();
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
            this.logger.LogDebug("Use Auth...");

            // Enable jwt authentication
            app.UseAuthentication();
        }

        /// <summary>
        /// Configures the app to use x-forwarded-for headers to obtain the real client IP.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        public void UseForwardHeaders(IApplicationBuilder app)
        {
            const string XForwardedProto = "X-Forwarded-Proto";

            IConfigurationSection section = this.configuration.GetSection("ForwardProxies");
            bool enabled = section.GetValue<bool>("Enabled");
            this.logger.LogInformation($"Forward Headers enabled: {enabled}");
            if (enabled)
            {
                string basePath = section.GetValue<string>("BasePath");
                if (!string.IsNullOrEmpty(basePath))
                {
                    this.logger.LogInformation($"Forward BasePath is set to {basePath}, setting PathBase for app");
                    app.UsePathBase(basePath);
                    app.Use(async (context, next) =>
                    {
                        context.Request.PathBase = basePath;
                        await next.Invoke().ConfigureAwait(true);
                    });
                    app.UsePathBase(basePath);
                }

                string[] proxyIPs = section.GetSection("IPs").Get<string[]>();
                ForwardedHeadersOptions options = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.All,
                    RequireHeaderSymmetry = false,
                    ForwardLimit = null,
                };

                app.Use((context, next) =>
                {
                    // IF this is not done, identity provider redirect urls drop to http:// which is undesirable.
                    if (context.Request.Headers.TryGetValue(XForwardedProto, out StringValues proto))
                    {
                        this.logger.LogInformation($"Client using protocol: {proto}, assigning to request protocol");
                        context.Request.Protocol = proto;
                    }
                    else
                    {
                        this.logger.LogInformation($"No header XforwardProto was found in request context - defaulting to {Uri.UriSchemeHttps}");
                        context.Request.Protocol = Uri.UriSchemeHttps;
                    }

                    return next();
                });

                this.logger.LogInformation("Enabling Use Forward Header");
                app.UseForwardedHeaders(options);
            }
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
                    HotModuleReplacementEndpoint = "/dist/dist/__webpack_hmr",
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
            this.logger.LogDebug("Use Swagger...");

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            app.UseSwaggerDocuments();
        }

        /// <summary>
        /// Handles authentication failures.
        /// </summary>
        /// <param name="context">The authentication failed context.</param>
        /// <param name="auditLogger">The audit logger provider.</param>
        /// <returns>An async task.</returns>
        private Task OnAuthenticationFailed(AuthenticationFailedContext context, IAuditLogger auditLogger)
        {
            this.logger.LogDebug("OnAuthenticationFailed...");

            AuditEvent auditEvent = new AuditEvent();
            auditEvent.AuditEventDateTime = DateTime.UtcNow;
            auditEvent.TransactionDuration = 0; // There's not a way to calculate the duration here.

            auditLogger.PopulateWithHttpContext(context.HttpContext, auditEvent);

            auditEvent.TransactionResultCode = Database.Constant.AuditTransactionResult.Unauthorized;
            auditEvent.CreatedBy = nameof(StartupConfiguration);
            auditEvent.CreatedDateTime = DateTime.UtcNow;

            auditLogger.WriteAuditEvent(auditEvent);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                State = "AuthenticationFailed",
                Message = context.Exception.ToString(),
            }));
        }
    }
}
