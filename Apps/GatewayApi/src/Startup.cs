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
namespace HealthGateway.GatewayApi
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.AccountDataAccess;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.MapProfiles;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Common.Utils.Phsa;
    using HealthGateway.Database.Delegates;
    using HealthGateway.GatewayApi.Api;
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
            StartupConfiguration.ConfigureProblemDetails(services);
            this.startupConfig.ConfigureForwardHeaders(services);
            this.startupConfig.ConfigureDatabaseServices(services);
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);
            this.startupConfig.ConfigureHangfireQueue(services);
            this.startupConfig.ConfigurePatientAccess(services);
            this.startupConfig.ConfigurePhsaV2Access(services);
            this.startupConfig.ConfigureTracing(services);
            this.startupConfig.ConfigureAccessControl(services);
            this.startupConfig.ConfigureMessaging(services);

            // Add services
            services.AddTransient<IApplicationSettingsService, ApplicationSettingsService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IDependentService, DependentService>();
            services.AddTransient<IEmailQueueService, EmailQueueService>();
            services.AddTransient<IGatewayApiCommunicationService, GatewayApiCommunicationService>();
            services.AddTransient<IGatewayApiMappingService, GatewayApiMappingService>();
            services.AddTransient<ILegalAgreementService, LegalAgreementService>();
            services.AddTransient<ILegalAgreementServiceV2, LegalAgreementServiceV2>();
            services.AddTransient<INoteService, NoteService>();
            services.AddTransient<INotificationSettingsService, NotificationSettingsService>();
            services.AddTransient<IPatientDetailsService, PatientDetailsService>();
            services.AddTransient<IPersonalAccountsService, PersonalAccountsService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<IUserEmailService, UserEmailService>();
            services.AddTransient<IUserEmailServiceV2, UserEmailServiceV2>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IUserPreferenceService, UserPreferenceService>();
            services.AddTransient<IUserPreferenceServiceV2, UserPreferenceServiceV2>();
            services.AddTransient<IUserProfileService, UserProfileService>();
            services.AddTransient<IUserProfileServiceV2, UserProfileServiceV2>();
            services.AddTransient<IUserSmsService, UserSmsService>();
            services.AddTransient<IUserSmsServiceV2, UserSmsServiceV2>();
            services.AddTransient<IWebAlertService, WebAlertService>();
            services.AddTransient<IUserValidationService, UserValidationService>();
            services.AddTransient<IRegistrationService, RegistrationService>();
            services.AddTransient<IUserProfileModelService, UserProfileModelService>();
            services.AddTransient<IMessagingVerificationService, MessagingVerificationService>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IDataAccessService, DataAccessService>();
            services.AddTransient<IUserProfileNotificationSettingService, UserProfileNotificationSettingService>();

            // Add delegates
            services.AddTransient<IApplicationSettingsDelegate, DbApplicationSettingsDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<ICDogsDelegate, CDogsDelegate>();
            services.AddTransient<ICommentDelegate, DbCommentDelegate>();
            services.AddTransient<ICommunicationDelegate, DbCommunicationDelegate>();
            services.AddTransient<ICryptoDelegate, AesCryptoDelegate>();
            services.AddTransient<IDelegationDelegate, DbDelegationDelegate>();
            services.AddTransient<IEmailDelegate, DbEmailDelegate>();
            services.AddTransient<IFeedbackDelegate, DbFeedbackDelegate>();
            services.AddTransient<ILegalAgreementDelegate, DbLegalAgreementDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            services.AddTransient<INoteDelegate, DbNoteDelegate>();
            services.AddTransient<IRatingDelegate, DbRatingDelegate>();
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
            services.AddTransient<IUserPreferenceDelegate, DbUserPreferenceDelegate>();
            services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();
            services.AddTransient<IUserProfileNotificationSettingDelegate, DbUserProfileNotificationSettingDelegate>();

            // Add API Clients
            CDogsConfig cdogsConfig = new();
            this.startupConfig.Configuration.Bind(CDogsConfig.CDogsConfigSectionKey, cdogsConfig);
            string cdogsEndpoint = cdogsConfig.BaseEndpoint;
            services.AddRefitClient<ICDogsApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(cdogsEndpoint));

            PhsaConfigV2 phsaConfig = new();
            this.startupConfig.Configuration.Bind(PhsaConfigV2.ConfigurationSectionKey, phsaConfig);

            services.AddRefitClient<IPersonalAccountsApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.AddRefitClient<IWebAlertApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.AddPatientRepositoryConfiguration(new AccountDataAccessConfiguration(phsaConfig.BaseUrl));

            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));

            services.AddAutoMapper(typeof(Startup), typeof(MessagingVerificationProfile));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ExceptionHandling.UseProblemDetails(app);
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app, false);
            this.startupConfig.UseAuth(app);
            this.startupConfig.EnrichTracing(app);
            this.startupConfig.UseRest(app);

            DisableTraceMethod(app);
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
