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
            IConfiguration configuration = builder.Configuration;
            ILogger logger = ProgramConfiguration.GetInitialLogger(configuration);
            IWebHostEnvironment environment = builder.Environment;

            // Add services to the container.
            _ = services.AddControllersWithViews();

            AddModules(services, configuration, logger, environment);
            AddServices(services, configuration);
            AddDelegates(services);

            // Add Refit clients
            PhsaConfigV2 phsaConfig = new();
            configuration.Bind(PhsaConfigV2.AdminConfigurationSectionKey, phsaConfig);

            _ = services.AddRefitClient<ISystemBroadcastApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            Uri? baseUri = configuration.GetValue<Uri>("KeycloakAdmin:BaseUrl");
            _ = services.AddRefitClient<IKeycloakAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = baseUri);

            _ = services.AddAutoMapper(typeof(Program), typeof(BroadcastProfile), typeof(UserProfileProfile), typeof(MessagingVerificationProfile));

            WebApplication app = builder.Build();
            RequestLoggingSettings requestLoggingSettings = new();
            configuration.GetSection("RequestLogging").Bind(requestLoggingSettings);
            if (requestLoggingSettings.Enabled)
            {
                _ = app.UseDefaultHttpRequestLogging(requestLoggingSettings.ExcludedPaths?.ToArray());
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

            _ = app.UseBlazorFrameworkFiles();

            HttpWeb.UseHttp(app, logger, configuration, environment, true, false);
            Auth.UseAuth(app, logger);
            _ = app.MapRazorPages();
            _ = app.MapControllers();
            _ = app.MapFallbackToFile("index.html");

            await app.RunAsync().ConfigureAwait(true);
        }

        private static void AddModules(IServiceCollection services, IConfiguration configuration, ILogger logger, IWebHostEnvironment environment)
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
            ExceptionHandling.ConfigureProblemDetails(services, environment);
            MessageBus.ConfigureMessageBus(services, configuration);
        }

        private static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddTransient<IBroadcastService, BroadcastService>();
            _ = services.AddTransient<IConfigurationService, ConfigurationService>();
            _ = services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            _ = services.AddTransient<IDashboardService, DashboardService>();
            _ = services.AddTransient<ICommunicationService, CommunicationService>();
            _ = services.AddTransient<ICsvExportService, CsvExportService>();
            _ = services.AddTransient<IInactiveUserService, InactiveUserService>();
            _ = services.AddTransient<ISupportService, SupportService>();
            _ = services.AddTransient<IAgentAccessService, AgentAccessService>();
            _ = services.AddTransient<IDelegationService, DelegationService>();
            _ = services.AddPatientRepositoryConfiguration(new AccountDataAccessConfiguration(configuration.GetSection("PhsaV2:BaseUrl").Get<Uri>()!));
        }

        private static void AddDelegates(IServiceCollection services)
        {
            _ = services.AddTransient<IDelegationDelegate, DbDelegationDelegate>();
            _ = services.AddTransient<IMessagingVerificationDelegate, DbMessagingVerificationDelegate>();
            _ = services.AddTransient<IFeedbackDelegate, DbFeedbackDelegate>();
            _ = services.AddTransient<IRatingDelegate, DbRatingDelegate>();
            _ = services.AddTransient<IUserProfileDelegate, DbProfileDelegate>();
            _ = services.AddTransient<ICommunicationDelegate, DbCommunicationDelegate>();
            _ = services.AddTransient<INoteDelegate, DbNoteDelegate>();
            _ = services.AddTransient<IResourceDelegateDelegate, DbResourceDelegateDelegate>();
            _ = services.AddTransient<ICommentDelegate, DbCommentDelegate>();
            _ = services.AddTransient<IAdminTagDelegate, DbAdminTagDelegate>();
            _ = services.AddTransient<IFeedbackTagDelegate, DbFeedbackTagDelegate>();
            _ = services.AddTransient<IVaccineProofDelegate, VaccineProofDelegate>();
            _ = services.AddTransient<IAdminUserProfileDelegate, DbAdminUserProfileDelegate>();
            _ = services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
        }
    }
}
