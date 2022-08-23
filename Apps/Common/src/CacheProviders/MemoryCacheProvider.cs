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
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// Provides a cache Provider for InMemory Cache.
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
        /// </summary>
        /// <param name="memoryCache">The injected memory cache.</param>
        public MemoryCacheProvider(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public T? GetItem<T>(string key)
            where T : class
        {
            this.memoryCache.TryGetValue(key, out T? cacheItem);
            return cacheItem;
        }

        /// <inheritdoc/>
        public void AddItem<T>(string key, T value, TimeSpan? expiry = null)
            where T : class
        {
            MemoryCacheEntryOptions cacheEntryOptions = new()
            {
                AbsoluteExpirationRelativeToNow = expiry,
            };
            this.memoryCache.Set(key, value, cacheEntryOptions);
        }

        /// <inheritdoc/>
        public void RemoveItem(string key)
        {
            this.memoryCache.Remove(key);
        }
    }
}
