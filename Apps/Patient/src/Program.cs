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
namespace HealthGateway.HGAdmin.Server
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
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
            IConfiguration configuration = builder.Configuration;
            ILogger logger = ProgramConfiguration.GetInitialLogger(configuration);
            IWebHostEnvironment environment = builder.Environment;

            Patient.ConfigurePatientExceptionHandling(services, environment);
            HttpWeb.ConfigureForwardHeaders(services, logger, configuration);
            Db.ConfigureDatabaseServices(services, logger, configuration);
            HttpWeb.ConfigureHttpServices(services, logger);
            Audit.ConfigureAuditServices(services, logger, configuration);
            Auth.ConfigureAuthServicesForJwtBearer(services, logger, configuration, environment);
            Auth.ConfigureAuthorizationServices(services, logger, configuration);
            SwaggerDoc.ConfigureSwaggerServices(services, configuration);
            Patient.ConfigurePatientAccess(services, logger, configuration);

            // POC V2 Patient Access
            services.AddTransient<IClientRegistriesDelegateV2, ClientRegistriesDelegateV2>();
            services.AddTransient<IPatientServiceV2, PatientServiceV2>();

            Utility.ConfigureTracing(services, logger, configuration);

            WebApplication app = builder.Build();
            Patient.ConfigurePatientExceptionHandling(app, environment);

            HttpWeb.UseForwardHeaders(app, logger, configuration);
            SwaggerDoc.UseSwagger(app, logger);
            HttpWeb.UseHttp(app, logger, configuration, environment, false, false);
            Auth.UseAuth(app, logger);
            HttpWeb.UseRest(app, logger);

            await app.RunAsync().ConfigureAwait(true);
        }
    }
}
