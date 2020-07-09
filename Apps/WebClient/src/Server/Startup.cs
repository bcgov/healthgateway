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
namespace HealthGateway.WebClient
{
    using System;
    using System.Diagnostics.Contracts;
    using Hangfire;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using VueCliMiddleware;

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
        /// <param name="env">The environment variables provider.</param>
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
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);

            // Add services
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IUserEmailService, UserEmailService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IBetaRequestService, BetaRequestService>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<INoteService, NoteService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IUserSMSService, UserSMSService>();
            services.AddTransient<INotificationSettingsService, NotificationSettingsService>();
            services.AddTransient<IUserPreferenceDelegate , DBUserPreferenceDelegate>();

            // Add delegates
            services.AddTransient<IUserProfileDelegate, DBProfileDelegate>();
            services.AddTransient<IUserPreferenceDelegate, DBUserPreferenceDelegate>();
            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DBMessagingVerificationDelegate>();
            services.AddTransient<IFeedbackDelegate, DBFeedbackDelegate>();
            services.AddTransient<IBetaRequestDelegate, DBBetaRequestDelegate>();
            services.AddTransient<ILegalAgreementDelegate, DBLegalAgreementDelegate>();
            services.AddTransient<INoteDelegate, DBNoteDelegate>();
            services.AddTransient<ICommentDelegate, DBCommentDelegate>();
            services.AddTransient<ICryptoDelegate, AESCryptoDelegate>();
            services.AddTransient<ICommunicationDelegate, DBCommunicationDelegate>();
            services.AddTransient<INotificationSettingsDelegate, RestNotificationSettingsDelegate>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

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

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Contract.Requires(env != null);

            app.UseSpaStaticFiles();

            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseContentSecurityPolicy(app);
            this.startupConfig.UseAuth(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (!env.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            bool debugerAttached = System.Diagnostics.Debugger.IsAttached;

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                if (env.IsDevelopment() && debugerAttached)
                {
                    endpoints.MapToVueCliProxy(
                        "{*path}",
                        new SpaOptions { SourcePath = "ClientApp" },
                        npmScript: "serve",
                        port:8585,
                        regex: "Compiled successfully",
                        forceKill: true);
                }
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment() && !debugerAttached)
                {
                    // change this to whatever webpack dev server says it's running on
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                }
            });

            bool redirectToWWW = this.configuration.GetSection("WebClient").GetValue<bool>("RedirectToWWW");
            if (redirectToWWW)
            {
                RewriteOptions rewriteOption = new RewriteOptions()
                    .AddRedirectToWwwPermanent();
                app.UseRewriter(rewriteOption);
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
        }
    }
}
