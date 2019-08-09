//-------------------------------------------------------------------------
// Copyright � 2019 Province of British Columbia
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
namespace HealthGateway.WebClient
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
    using HealthGateway.WebClient.Swagger;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using Swashbuckle.AspNetCore.SwaggerUI;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.logger.LogDebug("Starting Service Configuration...");
            services.AddHttpClient();
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(o =>
             {
                 this.configuration.GetSection("OpenIdConnect").Bind(o);
                 if (string.IsNullOrEmpty(o.Authority))
                 {
                     this.logger.LogCritical("OpenIdConnect Authority is missing, bad things are going to occur");
                 }

                 o.Events = new OpenIdConnectEvents()
                 {
                     OnTokenValidated = ctx =>
                     {
                         JwtSecurityToken accessToken = ctx.SecurityToken;
                         if (accessToken != null)
                         {
                             ClaimsIdentity identity = ctx.Principal.Identity as ClaimsIdentity;
                             if (identity != null)
                             {
                                 identity.AddClaim(new Claim("access_token", accessToken.RawData));
                             }
                         }

                         return Task.CompletedTask;
                     },
                     OnRedirectToIdentityProvider = ctx =>
                     {
                         if (ctx.Properties.Items.ContainsKey(this.configuration["KeyCloak:IDPHintKey"]))
                         {
                             this.logger.LogDebug("Adding IDP Hint passed in from client to provider");
                             ctx.ProtocolMessage.SetParameter(
                                    this.configuration["KeyCloak:IDPHintKey"], ctx.Properties.Items[this.configuration["KeyCloak:IDPHintKey"]]);
                         }
                         else
                         {
                             if (!string.IsNullOrEmpty(this.configuration["KeyCloak:IDPHint"]))
                             {
                                 this.logger.LogDebug("Adding IDP Hint on Redirect to provider");
                                 ctx.ProtocolMessage.SetParameter(this.configuration["KeyCloak:IDPHintKey"], this.configuration["KeyCloak:IDPHint"]);
                             }
                         }

                         return Task.FromResult(0);
                     },
                     OnAuthenticationFailed = c =>
                     {
                         c.HandleResponse();
                         c.Response.StatusCode = 500;
                         c.Response.ContentType = "text/plain";
                         this.logger.LogError(c.Exception.ToString());
                         return c.Response.WriteAsync(c.Exception.ToString());
                     },
                 };
             });

            // Imms Service
            services.AddTransient<IImmsService>(serviceProvider =>
            {
                this.logger.LogDebug("Configuring Transient Service IImmsService");
                IImmsService service = new ImmsService(
                    serviceProvider.GetService<ILogger<AuthService>>(),
                    serviceProvider.GetService<IHttpContextAccessor>(),
                    serviceProvider.GetService<IConfiguration>(),
                    serviceProvider.GetService<IHttpClientFactory>(),
                    serviceProvider.GetService<IAuthService>());
                return service;
            });

            // Auth Service
            services.AddTransient<IAuthService>(serviceProvider =>
            {
                this.logger.LogDebug("Configuring Transient Service IAuthService");
                IAuthService service = new AuthService(
                    serviceProvider.GetService<ILogger<AuthService>>(),
                    serviceProvider.GetService<IHttpContextAccessor>(),
                    serviceProvider.GetService<IConfiguration>());
                return service;
            });

            // Inject HttpContextAccessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<SwaggerSettings>(this.configuration.GetSection(nameof(SwaggerSettings)));

            services
                .AddApiVersionWithExplorer()
                .AddSwaggerOptions()
                .AddSwaggerGen();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Console.WriteLine(env.EnvironmentName);
            if (env.IsDevelopment())
            {
                this.logger.LogDebug("Application is running in development mode");
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

            // forwarded Header middleware required for apps behind proxies and load balancers
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            });

            app.UseSwaggerDocuments();
            app.UseResponseCompression();
            app.UseAuthentication();
            app.UseCookiePolicy();

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
    }
}