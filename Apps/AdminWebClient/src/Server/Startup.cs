//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace HealthGateway.AdminWebClient
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AspNetConfiguration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.Extensions.Logging;
    using VueCliMiddleware;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.logger = this.startupConfig.Logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

            this.logger.LogDebug("Configure Services...");

            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            //this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.ConfigureAuthenticationService(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);

            // Add services
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IBetaRequestService, BetaRequestService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();

            // Add delegates
            services.AddTransient<IBetaRequestDelegate, DBBetaRequestDelegate>();
            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IEmailInviteDelegate, DBEmailInviteDelegate>();

            // Configure SPA 
            services.AddControllersWithViews();

            // In production, the Vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));
            JobStorage.Current = new PostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseAuth(app);


            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            if (env.IsDevelopment())
            {
                this.logger.LogInformation("ENVIRONENT is Development");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            /*app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy(); // Before UseAuthentication or anything else that writes cookies. 

            // app.UseRequestLocalization();
            //app.UseCors(CorsPolicy);

            app.UseAuthentication();
            app.UseAuthorization();*/

            if (!env.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller}/{action=Index}/{id?}");

                    if (env.IsDevelopment())
                    {
                        endpoints.MapToVueCliProxy(
                            "{*path}",
                            new SpaOptions { SourcePath = "ClientApp" },
                            npmScript: (System.Diagnostics.Debugger.IsAttached) ? "serve" : null,
                            regex: "Compiled successfully",
                            forceKill: true
                            );
                    }

                    // Add MapRazorPages if the app uses Razor Pages. Since Endpoint Routing includes support for many frameworks, adding Razor Pages is now opt -in.
                    //endpoints.MapRazorPages();
                }
            );

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });
        }

        private void ConfigureAuthenticationService(IServiceCollection services)
        {
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
                     OnTicketReceived = e =>
                    {
                        
                        return Task.CompletedTask;
                    },
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
        }
    }
}
