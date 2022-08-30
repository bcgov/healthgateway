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
    using System.Linq;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// Provides ASP.Net Services related to Caching.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class GatewayCache
    {
        /// <summary>
        /// Configures the Cache services.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureCaching(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            if (services.All(x => x.ServiceType != typeof(ICacheProvider)))
            {
                logger.LogInformation("Configure Caching...");
                EnableRedis(services, logger, configuration);
                services.AddSingleton<ICacheProvider, RedisCacheProvider>();
            }
            else
            {
                logger.LogInformation("Caching previously configured skipping...");
            }
        }

        /// <summary>
        /// Enables the Redis Multiplexer.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void EnableRedis(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            if (services.All(x => x.ServiceType != typeof(IConnectionMultiplexer)))
            {
                string? connectionString = configuration.GetValue<string>("RedisConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogWarning("Cache Connection string is null/empty and caching likely broken");
                }

                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
            }
            else
            {
                logger.LogInformation("Redis Multiplexer previously configured, skipping...");
            }
        }
    }
}
