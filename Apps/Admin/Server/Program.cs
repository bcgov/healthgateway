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

#pragma warning disable S1118 // Utility classes should not have public constructors
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
#pragma warning disable CA1506 // Avoid excessive class coupling

namespace HealthGateway.Admin.Server
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.MapProfiles;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils.Phsa;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Refit;
    using CommunicationService = HealthGateway.Admin.Server.Services.CommunicationService;
    using ICommunicationService = HealthGateway.Admin.Server.Services.ICommunicationService;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        /// <summary>
        /// The entry point for the class.
        /// </summary>
        /// <param name="args">The command line arguments to be passed in.</param>
        /// <returns>A task which represents the exit of the application.</returns>
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = ProgramConfiguration.CreateWebAppBuilder(args);

            IServiceCollection services = builder.Services;
            ConfigurationManager configuration = builder.Configuration;
            ILogger logger = ProgramConfiguration.GetInitialLogger(configuration);
            IWebHostEnvironment environment = builder.Environment;

            // Add services to the container.
            services.AddControllersWithViews();

            AddModules(services, configuration, logger, environment);
            AddServices(services, configuration);
            AddDelegates(services);

            // Add Refit clients
            PhsaConfigV2 phsaConfigV2 = new();
            configuration.Bind(PhsaConfigV2.AdminConfigurationSectionKey, phsaConfigV2);

            services.AddRefitClient<ISystemBroadcastApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfigV2.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            services.AddRefitClient<IHgAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfigV2.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            Uri? baseUri = configuration.GetValue<Uri>("KeycloakAdmin:BaseUrl");
            services.AddRefitClient<IKeycloakAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = baseUri);

            PhsaConfig phsaConfigV1 = new();
            configuration.Bind(PhsaConfig.ConfigurationSectionKey, phsaConfigV1);
            services.AddRefitClient<IImmunizationAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfigV1.BaseUrl);

            services.AddAutoMapper(typeof(Program), typeof(BroadcastProfile), typeof(MessagingVerificationProfile));

            WebApplication app = builder.Build();
            RequestLoggingSettings requestLoggingSettings = new();
            configuration.GetSection("RequestLogging").Bind(requestLoggingSettings);
            if (requestLoggingSettings.Enabled)
            {
                app.UseDefaultHttpRequestLogging(requestLoggingSettings.ExcludedPaths?.ToArray());
            }

            ExceptionHandling.UseProblemDetails(app);
            HttpWeb.UseForwardHeaders(app, logger, configuration);
            HttpWeb.UseContentSecurityPolicy(app, configuration);
            SwaggerDoc.UseSwagger(app, logger);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }

            app.UseBlazorFrameworkFiles();

            HttpWeb.UseHttp(app, logger, configuration, environment, true, false);
            Auth.UseAuth(app, logger);
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }

        private static void AddModules(IServiceCollection services, ConfigurationManager configuration, ILogger logger, IWebHostEnvironment environment)
        {
            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            Db.ConfigureDatabaseServices(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Audit.ConfigureAuditServices(services, logger, configuration);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment, "preferred_username");
            Auth.ConfigureAuthorizationServices(services, logger, configuration);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            JobScheduler.ConfigureHangfireQueue(services, configuration);
            Patient.ConfigurePatientAccess(services, logger, configuration);
            PhsaV2.ConfigurePhsaV2Access(services, logger, configuration, PhsaConfigV2.AdminConfigurationSectionKey);
            ExceptionHandling.ConfigureProblemDetails(services);
            MessageBus.ConfigureMessageBus(services, configuration);
            Utility.ConfigureTracing(services, logger, configuration);
        }

        private static void AddServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddTransient<IAdminServerMappingService, AdminServerMappingService>();
            services.AddTransient<ICommonMappingService, CommonMappingService>();
            services.AddTransient<IBroadcastService, BroadcastService>();
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<ICsvExportService, CsvExportService>();
            services.AddTransient<IInactiveUserService, InactiveUserService>();
            services.AddTransient<ISupportService, SupportService>();
            services.AddTransient<IAgentAccessService, AgentAccessService>();
            services.AddTransient<IDelegationService, DelegationService>();
            services.AddTransient<IAdminReportService, AdminReportService>();
            services.AddTransient<IBetaFeatureService, BetaFeatureService>();
            services.AddPatientRepositoryConfiguration(new AccountDataAccessConfiguration(configuration.GetSection("PhsaV2Admin:BaseUrl").Get<Uri>()!));
        }

        private static void AddDelegates(IServiceCollection services)
        {
            services.AddTransient<IDelegationDelegate, DbDelegationDelegate>();
            services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            services.AddTransient<IFeedbackDelegate, DbFeedbackDelegate>();
            services.AddTransient<IRatingDelegate, DbRatingDelegate>();
            services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();
            services.AddTransient<ICommunicationDelegate, DbCommunicationDelegate>();
            services.AddTransient<INoteDelegate, DbNoteDelegate>();
            services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
            services.AddTransient<ICommentDelegate, DbCommentDelegate>();
            services.AddTransient<IAdminTagDelegate, DbAdminTagDelegate>();
            services.AddTransient<IVaccineProofDelegate, VaccineProofDelegate>();
            services.AddTransient<IAdminUserProfileDelegate, DbAdminUserProfileDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<IImmunizationAdminDelegate, RestImmunizationAdminDelegate>();
            services.AddTransient<IVaccineStatusDelegate, RestVaccineStatusDelegate>();
            services.AddTransient<IBetaFeatureAccessDelegate, DbBetaFeatureAccessDelegate>();
        }
    }
}
