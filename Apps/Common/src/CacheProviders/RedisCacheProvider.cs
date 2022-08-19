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
namespace HealthGateway.Common.CacheProviders
{
    using System;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// Provides a cache Provider for Redis.
    /// </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly ILogger<RedisCacheProvider> logger;
        private readonly IConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="connectionMultiplexer">The injected connection multiplexer.</param>
        public RedisCacheProvider(ILogger<RedisCacheProvider> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            this.logger = logger;
            this.connectionMultiplexer = connectionMultiplexer;
        }

        /// <inheritdoc/>
        public T? GetItem<T>(string key)
            where T : class
        {
            T? cacheItem = null;
            string? cacheJson = this.GetItem(key);
            if (cacheJson != null)
            {
                cacheItem = JsonSerializer.Deserialize<T>(cacheJson);
            }

            return cacheItem;
        }

        /// <inheritdoc/>
        public void AddItem<T>(string key, T value, TimeSpan? expiry = null)
            where T : class
        {
            this.Add(key, JsonSerializer.Serialize(value), expiry);
        }

        /// <inheritdoc/>
        public void RemoveItem(string key)
        {
            this.connectionMultiplexer.GetDatabase().KeyDelete(key, flags: CommandFlags.FireAndForget);
        }

        private void Add(string key, string value, TimeSpan? expiry = null)
        {
            this.connectionMultiplexer.GetDatabase().StringSet(key, value, expiry, flags: CommandFlags.FireAndForget);
        }

        private string? GetItem(string key)
        {
            string? cacheStr = null;
            try
            {
                cacheStr = this.connectionMultiplexer.GetDatabase().StringGet(key);
            }
            catch (RedisTimeoutException e)
            {
                this.logger.LogInformation($"Unable to retrieve cache key {key}\n{e}");
            }

            return cacheStr;
        }
    }
}
