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
namespace HealthGateway.WebClient
{
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configures the application during startup.
    /// </summary>
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The environment variables provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            this.startupConfig = new StartupConfiguration(configuration, env, logger);
            this.configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The injected services provider.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureHttpServices(services);
            this.startupConfig.ConfigureAuditServices(services);
            this.startupConfig.ConfigureAuthServicesForJwtBearer(services);
            this.startupConfig.ConfigureAuthorizationServices(services);
            this.startupConfig.ConfigureSwaggerServices(services);

            services.AddDbContext<GatewayDbContext>(options => options.UseNpgsql(
                this.configuration.GetConnectionString("GatewayConnection")));
            services.AddTransient<IProfileDelegate, ProfileDelegate>();
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient<IUserProfileService, UserProfileService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Contract.Requires(env != null);
            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseSwagger(app);
            this.startupConfig.UseHttp(app);
            this.startupConfig.UseWebClient(app);
        }
    }
}