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
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// Provides a cache Provider for Redis.
    /// </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly ILogger<RedisCacheProvider> logger;

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
        {
            T? cacheItem = default;
            string? cacheJson = this.GetItemJson(key);
            if (cacheJson != null)
            {
                cacheItem = JsonSerializer.Deserialize<T>(cacheJson);
            }

            return cacheItem;
        }

        /// <inheritdoc/>
        public void AddItem<T>(string key, T value, TimeSpan? expiry = null)
        {
            this.connectionMultiplexer.GetDatabase().StringSet(key, Serialize(value), expiry, flags: CommandFlags.FireAndForget);
        }

        /// <inheritdoc/>
        public void RemoveItem(string key)
        {
            this.connectionMultiplexer.GetDatabase().KeyDelete(key, CommandFlags.FireAndForget);
        }

        /// <inheritdoc/>
        public T? GetOrSet<T>(string key, Func<T?> valueGetter, TimeSpan? expiry = null)
        {
            T? item = this.GetItem<T>(key);
            if (item == null)
            {
                item = valueGetter();
                if (item != null)
                {
                    this.AddItem(key, item, expiry);
                }
            }

            return item;
        }

        /// <inheritdoc/>
        public async Task<T?> GetItemAsync<T>(string key, CancellationToken ct = default)
        {
            T? cacheItem = default;
            string? cacheJson = await this.GetItemJsonAsync(key, ct);
            if (cacheJson != null)
            {
                cacheItem = JsonSerializer.Deserialize<T>(cacheJson);
            }

            return cacheItem;
        }

        /// <inheritdoc/>
        public async Task AddItemAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            await this.connectionMultiplexer.GetDatabase().StringSetAsync(key, Serialize(value), expiry, flags: CommandFlags.FireAndForget);
        }

        /// <inheritdoc/>
        public async Task RemoveItemAsync(string key, CancellationToken ct = default)
        {
            await this.connectionMultiplexer.GetDatabase().KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }

        /// <inheritdoc/>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> valueGetter, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            T? item = await this.GetItemAsync<T>(key, ct);
            if (item == null)
            {
                item = await valueGetter();
                await this.AddItemAsync(key, item, expiry, ct);
            }

            return item;
        }

        private static string Serialize<T>(T value)
        {
            return value == null ? JsonSerializer.Serialize(value, typeof(T)) : JsonSerializer.Serialize(value, value.GetType());
        }

        private string? GetItemJson(string key)
        {
            string? cacheStr = null;
            try
            {
                cacheStr = this.connectionMultiplexer.GetDatabase().StringGet(key);
            }
            catch (RedisTimeoutException e)
            {
                this.logger.LogInformation("Unable to retrieve cache key {Key}\n{Exception}", key, e.ToString());
            }

            return cacheStr;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Team decision")]
        private async Task<string?> GetItemJsonAsync(string key, CancellationToken ct = default)
        {
            string? cacheStr = null;
            try
            {
                cacheStr = await this.connectionMultiplexer.GetDatabase().StringGetAsync(key);
            }
            catch (RedisTimeoutException e)
            {
                this.logger.LogInformation("Unable to retrieve cache key {Key}\n{Exception}", key, e.ToString());
            }

            return cacheStr;
        }
    }
}
