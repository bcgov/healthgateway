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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <param name="keyPrefix">an optional prefix that is appended to all keys.</param>
        public static void ConfigureCaching(IServiceCollection services, ILogger logger, IConfiguration configuration, string? keyPrefix = null)
        {
            EnableRedis(services, logger, configuration);
            services.TryAddSingleton<ICacheProvider, DistributedCacheProvider>();
        }

        /// <summary>
        /// Enables the Redis Multiplexer.
        /// </summary>
        /// <param name="services">The services collection provider.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void EnableRedis(IServiceCollection services, ILogger logger, IConfiguration configuration)
        {
            string? redisConnectionString = configuration.GetValue<string>("RedisConnection");

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis cache Connection string is null/empty and caching likely broken.");
            }

            logger.LogInformation("Configuring Redis cache");
            services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
            services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        }
    }
}
