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
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.DrugMaintainer;
    using Healthgateway.JobScheduler.Jobs;
    using Healthgateway.JobScheduler.Utils;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The startup class.
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
            this.configuration = configuration;
            this.logger = this.startupConfig.Logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The passed in Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            services.AddRazorPages();
            this.startupConfig.ConfigureOpenIdConnectServices(services);
            this.startupConfig.ConfigureAccessControl(services);

            string requiredUserRole = this.configuration.GetValue<string>("OpenIdConnect:UserRole");
            string userRoleClaimType = this.configuration.GetValue<string>("OpenIdConnect:RolesClaim");

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminUserPolicy", policy =>
                    {
                        policy.AddAuthenticationSchemes(OpenIdConnectDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(userRoleClaimType, requiredUserRole);
                        policy.RequireAssertion(ctx =>
                        {
                            return ctx.User.HasClaim(userRoleClaimType, requiredUserRole);
                        });
                    });
            });

            // Add Delegates and services for jobs
            services.AddTransient<IFileDownloadService, FileDownloadService>();
            services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
            services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();
            services.AddTransient<IApplicationSettingsDelegate, DBApplicationSettingsDelegate>();
            services.AddTransient<ILegalAgreementDelegate, DBLegalAgreementDelegate>();
            services.AddTransient<IUserProfileDelegate, DBProfileDelegate>();
            services.AddTransient<ICommunicationDelegate, DBCommunicationDelegate>();
            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DBMessagingVerificationDelegate>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<INotificationSettingsDelegate, RestNotificationSettingsDelegate>();
            services.AddTransient<INotificationSettingsService, NotificationSettingsService>();
            services.AddTransient<IResourceDelegateDelegate, DBResourceDelegateDelegate>();
            services.AddTransient<IEventLogDelegate, DBEventLogDelegate>();
            services.AddTransient<IFeedbackDelegate, DBFeedbackDelegate>();

            // Add injection for KeyCloak User Admin
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<IUserAdminDelegate, KeycloakUserAdminDelegate>();

            // Add Jobs
            services.AddTransient<FedDrugJob>();
            services.AddTransient<ProvincialDrugJob>();
            services.AddTransient<IEmailJob, EmailJob>();
            services.AddTransient<INotificationSettingsJob, NotificationSettingsJob>();
            services.AddTransient<IAdminFeedbackJob, AdminFeedbackJob>();

            // Enable Hangfire
            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));

            // Add processing server as IHostedService
            services.AddHangfireServer();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The passed in Application Builder.</param>
        /// <param name="env">The passed in Environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.logger.LogInformation($"Hosting Environment: {env!.EnvironmentName}");
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseContentSecurityPolicy(app);
            app.UseStaticFiles();
            this.startupConfig.UseAuth(app);

            if (!env.IsDevelopment())
            {
                app.UseResponseCompression();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard(string.Empty, new DashboardOptions
                {
                    DashboardTitle = this.configuration.GetValue<string>("DashboardTitle", "Hangfire Dashboard"),
                    AppPath = $"{this.configuration.GetValue<string>("JobScheduler:AdminHome")}",
                    Authorization = new List<IDashboardAuthorizationFilter> { }, // Very important to set this, or Authorization won't work.
                })
                .RequireAuthorization("AdminUserPolicy");
            });

            // Schedule Health Gateway Jobs
            BackgroundJob.Enqueue<DBMigrationsJob>(j => j.Migrate());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendLowPriorityEmail", j => j.SendLowPriorityEmails());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendStandardPriorityEmail", j => j.SendStandardPriorityEmails());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendHighPriorityEmail", j => j.SendHighPriorityEmails());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendUrgentPriorityEmail", j => j.SendUrgentPriorityEmails());
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedApprovedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedMarketedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedCancelledDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugJob>(this.configuration, "FedDormantDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<ProvincialDrugJob>(this.configuration, "PharmaCareDrugFile");
            SchedulerHelper.ScheduleJob<NotifyUpdatedLegalAgreementsJob>(this.configuration, "NotifyUpdatedLegalAgreements", j => j.Process());
            SchedulerHelper.ScheduleJob<CloseAccountJob>(this.configuration, "CloseAccounts", j => j.Process());
            SchedulerHelper.ScheduleJob<OneTimeJob>(this.configuration, "OneTime", j => j.Process());
            SchedulerHelper.ScheduleJob<CleanCacheJob>(this.configuration, "CleanCache", j => j.Process());
            SchedulerHelper.ScheduleJob<DeleteEmailJob>(this.configuration, "DeleteEmailJob", j => j.DeleteOldEmails());
        }
    }
}
