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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Utils;
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
        public T? GetOrSet<T>(string key, Func<T?> valueGetter, TimeSpan? expiry = null)
        {
            T? value = this.GetItem<T>(key);
            if (value == null)
            {
                value = valueGetter();
                if (value != null)
                {
                    this.AddItem(key, value, expiry);
                }
            }

            return value;
        }

        /// <inheritdoc/>
        public T? GetItem<T>(string key)
        {
            return this.cache.Get(this.KeyGen(key)).Deserialize<T?>();
        }

        /// <inheritdoc/>
        public void AddItem<T>(string key, T value, TimeSpan? expiry = null)
        {
            this.cache.Set(this.KeyGen(key), value.Serialize(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry });
        }

        /// <inheritdoc/>
        public void RemoveItem(string key)
        {
            this.cache.Remove(key);
        }

        /// <inheritdoc/>
        public async Task<T?> GetItemAsync<T>(string key, CancellationToken ct = default)
        {
            return (await this.cache.GetAsync(this.KeyGen(key), ct)).Deserialize<T?>();
        }

        /// <inheritdoc/>
        public async Task AddItemAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            await this.cache.SetAsync(this.KeyGen(key), value.Serialize(), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }, ct);
        }

        /// <inheritdoc/>
        public async Task RemoveItemAsync(string key, CancellationToken ct = default)
        {
            await this.cache.RemoveAsync(key, ct);
        }

        /// <inheritdoc/>
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> valueGetter, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            T? value = await this.GetItemAsync<T>(key, ct);
            if (value == null)
            {
                // cache miss
                value = await valueGetter();
                await this.AddItemAsync(key, value, expiry, ct);
            }

            return value;
        }

        private string KeyGen(string key)
        {
            return $"{this.keyPrefix}{key}";
        }
    }
}
