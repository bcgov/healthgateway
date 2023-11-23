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
namespace HealthGateway.Patient
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientDataAccess;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
            ConfigurationManager configuration = builder.Configuration;
            ILogger logger = ProgramConfiguration.GetInitialLogger(configuration);
            IWebHostEnvironment environment = builder.Environment;

            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            Db.ConfigureDatabaseServices(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Audit.ConfigureAuditServices(services, logger, configuration);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment);
            Auth.ConfigureAuthorizationServices(services, logger, configuration);
            Auth.ConfigureDelegateAuthorizationServices(services, logger, configuration);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            MessageBus.ConfigureMessageBus(services, configuration);

            Patient.ConfigurePatientAccess(services, logger, configuration);
            JobScheduler.ConfigureHangfireQueue(services, configuration);
            PersonalAccount.ConfigurePersonalAccountAccess(services, logger, configuration);

            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IPatientDataService, PatientDataService>();
            services.AddAutoMapper(typeof(Program));

            PhsaV2.ConfigurePhsaV2Access(services, logger, configuration);
            services.AddPatientDataAccess(new PatientDataAccessConfiguration(configuration.GetSection("PhsaV2:BaseUrl").Get<Uri>()!));
            services.AddPatientRepositoryConfiguration(new AccountDataAccessConfiguration(configuration.GetSection("PhsaV2:BaseUrl").Get<Uri>()!));

            Utility.ConfigureTracing(services, logger, configuration);

            ExceptionHandling.ConfigureProblemDetails(services, environment);

            WebApplication app = builder.Build();
            RequestLoggingSettings requestLoggingSettings = new();
            configuration.GetSection("RequestLogging").Bind(requestLoggingSettings);
            if (requestLoggingSettings.Enabled)
            {
                app.UseDefaultHttpRequestLogging(requestLoggingSettings.ExcludedPaths?.ToArray());
            }

            ExceptionHandling.UseProblemDetails(app);
            HttpWeb.UseForwardHeaders(app, logger, configuration);
            SwaggerDoc.UseSwagger(app, logger);
            HttpWeb.UseHttp(app, logger, configuration, environment, false, false);
            Auth.UseAuth(app, logger);
            HttpWeb.UseRest(app, logger);

            await app.RunAsync().ConfigureAwait(true);
        }
    }
}
