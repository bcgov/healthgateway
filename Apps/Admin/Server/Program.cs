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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// The entry point for the project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private const string PhsaConfigSectionKey = "PHSA";

        /// <summary>.
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

            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            Db.ConfigureDatabaseServices(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Audit.ConfigureAuditServices(services, logger);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment);
            Auth.ConfigureAuthorizationServices(services, logger, configuration);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            JobScheduler.ConfigureHangfireQueue(services, configuration);
            Patient.ConfigurePatientAccess(services, configuration);

            // Register Refit clients
            RegisterRefitClients(services, configuration);

            // Add services to the container.
            services.AddControllersWithViews();

            // Add HG Services
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IUserFeedbackService, UserFeedbackService>();
            services.AddTransient<IDashboardService, DashboardService>();
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<ICsvExportService, CsvExportService>();
            services.AddTransient<ICovidSupportService, CovidSupportService>();
            services.AddTransient<IInactiveUserService, InactiveUserService>();

            // Add HG Delegates
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
            services.AddTransient<IImmunizationAdminDelegate, RestImmunizationAdminDelegate>();
            services.AddTransient<IVaccineStatusDelegate, RestVaccineStatusDelegate>();
            services.AddTransient<IVaccineProofDelegate, VaccineProofDelegate>();
            services.AddTransient<IAdminUserProfileDelegate, DbAdminUserProfileDelegate>();
            services.AddTransient<IAuthenticationDelegate, AuthenticationDelegate>();
            services.AddTransient<IUserAdminDelegate, KeycloakUserAdminDelegate>();

            WebApplication app = builder.Build();

            HttpWeb.UseForwardHeaders(app, logger, configuration);
            UseHttp(app, logger, configuration, environment);
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

        /// <summary>
        /// Configures the app to use http.
        /// This is normally common code but the static files is changed here as per https://github.com/dotnet/aspnetcore/issues/25152 .
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="environment">The environment to use.</param>
        public static void UseHttp(IApplicationBuilder app, ILogger logger, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    if (context.File.Name == "service-worker-assets.js")
                    {
                        context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                        context.Context.Response.Headers.Add("Expires", "-1");
                    }

                    if (context.File.Name == "blazor.boot.json")
                    {
                        if (context.Context.Response.Headers.ContainsKey("blazor-environment"))
                        {
                            context.Context.Response.Headers.Remove("blazor-environment");
                        }

                        context.Context.Response.Headers.Add("blazor-environment", environment.EnvironmentName);
                    }
                },
            });
            app.UseRouting();

            // Enable health endpoint for readiness probe
            app.UseHealthChecks("/health");

            // Enable CORS
            string enableCors = configuration.GetValue("AllowOrigins", string.Empty);
            if (!string.IsNullOrEmpty(enableCors))
            {
                app.UseCors(builder =>
                {
                    builder
                        .WithOrigins(enableCors)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }

            app.UseResponseCompression();

            // Setup response secure headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                await next().ConfigureAwait(true);
            });

            // Enable Cache control and set defaults
            HttpWeb.UseResponseCaching(app, logger);
        }

        private static void RegisterRefitClients(IServiceCollection services, IConfiguration configuration)
        {
            PhsaConfig phsaConfig = new();
            configuration.Bind(PhsaConfigSectionKey, phsaConfig);
            services.AddRefitClient<IImmunizationAdminClient>()
                .ConfigureHttpClient(c => c.BaseAddress = phsaConfig.BaseUrl);
        }
    }
}
