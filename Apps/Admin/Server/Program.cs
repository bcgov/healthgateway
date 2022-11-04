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
namespace HealthGateway.Admin.Server
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
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
    public static class Program
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
            services.AddControllersWithViews();

            AddModules(services, configuration, logger, environment);
            AddServices(services);
            AddDelegates(services);

            // Add Refit clients
            PhsaConfigV2 phsaConfig = new();
            configuration.Bind(PhsaConfigV2.ConfigurationSectionKey, phsaConfig);

            services.AddRefitClient<ISystemBroadcastApi>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl)
                .AddHttpMessageHandler<AuthHeaderHandler>();

            Uri baseUri = configuration.GetValue<Uri>("KeycloakAdmin:BaseUrl");
            services.AddRefitClient<IKeycloakAdminApi>()
                .ConfigureHttpClient(c => c.BaseAddress = baseUri);

            services.AddAutoMapper(typeof(Program), typeof(BroadcastProfile), typeof(UserProfileProfile), typeof(MessagingVerificationProfile));

            WebApplication app = builder.Build();
            HttpWeb.UseForwardHeaders(app, logger, configuration);
            HttpWeb.UseHttp(app, logger, configuration, environment, true);
            HttpWeb.UseContentSecurityPolicy(app, configuration);
            SwaggerDoc.UseSwagger(app, logger);
            Auth.UseAuth(app, logger);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            await app.RunAsync().ConfigureAwait(true);
        }

        private static void AddModules(IServiceCollection services, IConfiguration configuration, ILogger logger, IWebHostEnvironment environment)
        {
            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            Db.ConfigureDatabaseServices(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Audit.ConfigureAuditServices(services, logger, configuration);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment);
            Auth.ConfigureAuthorizationServices(services, logger, configuration);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            JobScheduler.ConfigureHangfireQueue(services, configuration);
            Patient.ConfigurePatientAccess(services, logger, configuration);
            PhsaV2.ConfigurePhsaV2Access(services, logger, configuration);
        }

        private static void AddServices(IServiceCollection services)
        {
            services.AddTransient<IBroadcastService, BroadcastService>();
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<ICsvExportService, CsvExportService>();
            services.AddTransient<IInactiveUserService, InactiveUserService>();
            services.AddTransient<ISupportService, SupportService>();
        }

        private static void AddDelegates(IServiceCollection services)
        {
            services.AddTransient<IMessagingVerificationDelegate, DBMessagingVerificationDelegate>();
            services.AddTransient<IFeedbackDelegate, DBFeedbackDelegate>();
            services.AddTransient<IRatingDelegate, DBRatingDelegate>();
            services.AddTransient<IUserProfileDelegate, DBProfileDelegate>();
            services.AddTransient<ICommunicationDelegate, DBCommunicationDelegate>();
            services.AddTransient<INoteDelegate, DBNoteDelegate>();
            services.AddTransient<IResourceDelegateDelegate, DBResourceDelegateDelegate>();
            services.AddTransient<ICommentDelegate, DBCommentDelegate>();
            services.AddTransient<IAdminTagDelegate, DBAdminTagDelegate>();
            services.AddTransient<IFeedbackTagDelegate, DBFeedbackTagDelegate>();
            services.AddTransient<IVaccineStatusDelegate, RestVaccineStatusDelegate>();
            services.AddTransient<IVaccineProofDelegate, VaccineProofDelegate>();
            services.AddTransient<IAdminUserProfileDelegate, DbAdminUserProfileDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
        }
    }
}
