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
namespace HealthGateway.JobScheduler
{
    using System;
    using System.Collections.Generic;
    using Hangfire;
    using Hangfire.Dashboard;
    using Hangfire.PostgreSql;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Parsers;
    using HealthGateway.JobScheduler.AspNetConfiguration.Modules;
    using HealthGateway.JobScheduler.Jobs;
    using HealthGateway.JobScheduler.Listeners;
    using HealthGateway.JobScheduler.Utils;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
            this.configuration = configuration;
            this.logger = this.startupConfig.Logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The passed in Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            GatewayCache.ConfigureCaching(services, this.startupConfig.Logger, this.startupConfig.Configuration);
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            services.AddRazorPages();
            this.startupConfig.ConfigureOpenIdConnectServices(services);
            this.startupConfig.ConfigureAccessControl(services);
            this.startupConfig.ConfigureTracing(services);

            string? requiredUserRole = this.configuration.GetValue<string>("OpenIdConnect:UserRole");
            string? userRoleClaimType = this.configuration.GetValue<string>("OpenIdConnect:RolesClaim");

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy(
                        "AdminUserPolicy",
                        policy =>
                        {
                            policy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
                            policy.RequireAuthenticatedUser();
                            policy.RequireClaim(userRoleClaimType, requiredUserRole);
                            policy.RequireAssertion(ctx => ctx.User.HasClaim(userRoleClaimType, requiredUserRole));
                        });
                });

            // Add Delegates and services for jobs
            services.AddTransient<IFileDownloadService, FileDownloadService>();
            services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
            services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();
            services.AddTransient<IPharmacyAssessmentParser, PharmacyAssessmentParser>();
            services.AddTransient<IApplicationSettingsDelegate, DbApplicationSettingsDelegate>();
            services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();
            services.AddTransient<ICommunicationDelegate, DbCommunicationDelegate>();
            services.AddTransient<IEmailDelegate, DbEmailDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<INotificationSettingsDelegate, RestNotificationSettingsDelegate>();
            services.AddTransient<INotificationSettingsService, NotificationSettingsService>();
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
            services.AddTransient<IEventLogDelegate, DbEventLogDelegate>();
            services.AddTransient<IFeedbackDelegate, DbFeedbackDelegate>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IWriteAuditEventDelegate, DbWriteAuditEventDelegate>();

            // Add API Clients
            NotificationSettingsConfig notificationSettingsConfig = new();
            this.startupConfig.Configuration.Bind(NotificationSettingsConfig.NotificationSettingsConfigSectionKey, notificationSettingsConfig);
            services.AddRefitClient<INotificationSettingsApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(notificationSettingsConfig.Endpoint));

            GcNotify.Configure(services, this.logger, this.configuration);

            // Add injection for KeyCloak User Admin
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            Uri? baseUri = this.startupConfig.Configuration.GetValue<Uri>("KeycloakAdmin:BaseUrl");
            services.AddRefitClient<IKeycloakAdminApi>().ConfigureHttpClient(c => c.BaseAddress = baseUri);

            // Add Jobs
            services.AddTransient<FedDrugJob>();
            services.AddTransient<ProvincialDrugJob>();
            services.AddTransient<IEmailJob, EmailJob>();
            services.AddTransient<INotificationSettingsJob, NotificationSettingsJob>();
            services.AddTransient<IAdminFeedbackJob, AdminFeedbackJob>();

            // Enable Hangfire
            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));

            // Add processing server as IHostedService
            services.AddHangfireServer(opts => { opts.Queues = new[] { "default", AzureServiceBusSettings.OutboxQueueName }; });

            // Add Background Services
            services.AddHostedService<BannerListener>();

            GatewayCache.EnableRedis(services, this.logger, this.configuration);
            services.AddHostedService<AuditQueueListener>();
            MessageBus.ConfigureMessageBus(services, this.configuration);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The passed in Application Builder.</param>
        /// <param name="env">The passed in Environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseContentSecurityPolicy(app);
            this.startupConfig.UseAuth(app);

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapHangfireDashboard(
                            string.Empty,
                            new DashboardOptions
                            {
                                DashboardTitle = this.configuration.GetValue<string>("DashboardTitle", "Hangfire Dashboard"),
                                AppPath = $"{this.configuration.GetValue<string>("JobScheduler:AdminHome")}",
                                Authorization = new List<IDashboardAuthorizationFilter>(), // Very important to set this, or Authorization won't work.
                            })
                        .RequireAuthorization("AdminUserPolicy");
                });

            // Schedule Health Gateway Jobs
            BackgroundJob.Enqueue<DbMigrationsJob>(j => j.Migrate());
            SchedulerHelper.ScheduleJob<EmailJob>(this.configuration, "ResendEmails", j => j.SendEmails());
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedApprovedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedMarketedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedCancelledDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedDormantDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<ProvincialDrugJob>(this.configuration, "PharmaCareDrugFile");
            SchedulerHelper.ScheduleJob<CloseAccountJob>(this.configuration, "CloseAccounts", j => j.Process());
            SchedulerHelper.ScheduleJob<OneTimeJob>(this.configuration, "OneTime", j => j.Process());
            SchedulerHelper.ScheduleJob<DeleteEmailJob>(this.configuration, "DeleteEmailJob", j => j.DeleteOldEmails());
        }
    }
}
