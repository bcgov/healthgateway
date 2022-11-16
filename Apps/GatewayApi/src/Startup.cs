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
namespace HealthGateway.GatewayApi
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.MapProfiles;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The environment variables provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.startupConfig = new StartupConfiguration(configuration, env);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureHangfireQueue(services);
            this.startupConfig.ConfigurePatientAccess(services);
            this.startupConfig.ConfigureTracing(services);
            this.startupConfig.ConfigureAccessControl(services);

            // Add services
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IUserEmailService, UserEmailService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<INoteService, NoteService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IUserSmsService, UserSmsService>();
            services.AddTransient<INotificationSettingsService, NotificationSettingsService>();
            services.AddTransient<IDependentService, DependentService>();
            services.AddTransient<IUserPreferenceDelegate, DbUserPreferenceDelegate>();
            services.AddTransient<IReportService, ReportService>();

            // Add delegates
            services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();
            services.AddTransient<IUserPreferenceDelegate, DbUserPreferenceDelegate>();
            services.AddTransient<IEmailDelegate, DbEmailDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            services.AddTransient<IFeedbackDelegate, DbFeedbackDelegate>();
            services.AddTransient<IRatingDelegate, DbRatingDelegate>();
            services.AddTransient<ILegalAgreementDelegate, DbLegalAgreementDelegate>();
            services.AddTransient<INoteDelegate, DbNoteDelegate>();
            services.AddTransient<ICommentDelegate, DbCommentDelegate>();
            services.AddTransient<ICryptoDelegate, AesCryptoDelegate>();
            services.AddTransient<ICommunicationDelegate, DbCommunicationDelegate>();
            services.AddTransient<INotificationSettingsDelegate, RestNotificationSettingsDelegate>();
            services.AddTransient<IUserPreferenceDelegate, DbUserPreferenceDelegate>();
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
            services.AddTransient<ICDogsDelegate, CDogsDelegate>();

            // Add API Clients
            CDogsConfig cdogsConfig = new();
            this.startupConfig.Configuration.Bind(CDogsConfig.CDogsConfigSectionKey, cdogsConfig);
            string cdogsEndpoint = cdogsConfig.BaseEndpoint;
            if (cdogsConfig.DynamicServiceLookup)
            {
                cdogsEndpoint = ConfigurationUtility.ConstructServiceEndpoint(
                    cdogsConfig.BaseEndpoint,
                    $"{cdogsConfig.ServiceName}{cdogsConfig.ServiceHostSuffix}",
                    $"{cdogsConfig.ServiceName}{cdogsConfig.ServicePortSuffix}");
            }

            services.AddRefitClient<ICDogsApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(cdogsEndpoint));

            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));

            services.AddAutoMapper(typeof(Startup), typeof(UserProfileProfile));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseAuth(app);
            this.startupConfig.UseRest(app);

            DisableTraceMethod(app);
        }

        private static void DisableTraceMethod(IApplicationBuilder app)
        {
            app.Use(
                async (context, next) =>
                {
                    if (context.Request.Method == "TRACE")
                    {
                        context.Response.StatusCode = 405;
                        return;
                    }

                    await next.Invoke().ConfigureAwait(true);
                });
        }
    }
}
