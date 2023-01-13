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
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;

    /// <summary>
    /// Cache provider using built-in distributed cache.
    /// </summary>
    public class DistributedCacheProvider : ICacheProvider
    {
        private readonly IDistributedCache cache;
        private readonly string? keyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheProvider"/> class.
        /// </summary>
        /// <param name="cache">distributed cache instance.</param>
        /// <param name="keyPrefix">key prefix to add to all cached items.</param>
        public DistributedCacheProvider(IDistributedCache cache, string? keyPrefix = null)
        {
            this.cache = cache;
            this.keyPrefix = keyPrefix ?? string.Empty;
            if (!string.IsNullOrEmpty(keyPrefix))
            {
                this.keyPrefix += ":";
            }
        }

        /// <inheritdoc/>
        public T? GetOrSet<T>(string key, Func<T> valueGetter, TimeSpan? expiry = null)
        {
            T? value = this.GetItem<T>(key);
            if (value == null)
            {
                // cache miss
                value = valueGetter();
                this.AddItem(key, value, expiry);
            }

            return value;
        }

        /// <inheritdoc/>
        public T? GetItem<T>(string key)
        {
            return Deserialize<T?>(this.cache.Get(this.KeyGen(key)));
        }

        /// <inheritdoc/>
        public void AddItem<T>(string key, T value, TimeSpan? expiry = null)
        {
            this.cache.Set(this.KeyGen(key), Serialize(value), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry });
        }

        /// <inheritdoc/>
        public void RemoveItem(string key)
        {
            this.cache.Remove(key);
        }

        /// <inheritdoc/>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            return Deserialize<T?>(await this.cache.GetAsync(this.KeyGen(key)).ConfigureAwait(true));
        }

        /// <inheritdoc/>
        public async Task AddItemAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            await this.cache.SetAsync(this.KeyGen(key), Serialize(value), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task RemoveItemAsync(string key)
        {
            await this.cache.RemoveAsync(key).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> valueGetter, TimeSpan? expiry = null)
        {
            T? value = await this.GetItemAsync<T>(key).ConfigureAwait(true);
            if (value == null)
            {
                // cache miss
                value = await valueGetter().ConfigureAwait(true);
                await this.AddItemAsync(key, value, expiry).ConfigureAwait(true);
            }

            return value;
        }

        private static T? Deserialize<T>(byte[]? data)
        {
            return data == null || data.Length == 0 ? default : JsonSerializer.Deserialize<T?>(data);
        }

        private static byte[] Serialize<T>(T? obj)
        {
            return obj == null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        private string KeyGen(string key)
        {
            return $"{this.keyPrefix}{key}";
        }
    }
}
