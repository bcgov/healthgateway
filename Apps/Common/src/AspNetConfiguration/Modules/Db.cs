// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides ASP.Net Services related to Authentication and Authorization services.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Db
    {
        /// <summary>
        /// Configures the Database services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureDatabaseServices(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            logger.LogDebug("ConfigureDatabaseServices...");
            IConfigurationSection section = configuration.GetSection("Logging:SensitiveDataLogging");
            bool isSensitiveDataLoggingEnabled = section.GetValue("Enabled", false);
            logger.LogDebug($"Sensitive Data Logging is enabled: {isSensitiveDataLoggingEnabled}");

            services.AddDbContextPool<GatewayDbContext>(
                options =>
                {
                    options.UseNpgsql(configuration.GetConnectionString("GatewayConnection"));
                    if (isSensitiveDataLoggingEnabled)
                    {
                        options.EnableSensitiveDataLogging();
                    }
                });

            if (isSensitiveDataLoggingEnabled)
            {
                services.AddLogging(
                    loggingBuilder =>
                    {
                        loggingBuilder.AddConsole()
                            .AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information);
                    });
            }
        }
    }
}
