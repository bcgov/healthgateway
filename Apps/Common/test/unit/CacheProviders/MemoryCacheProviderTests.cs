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
namespace HealthGateway.CommonTests.CacheProviders
{
    using System;
    using System.Threading;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Caching.Memory;
    using Xunit;

    /// <summary>
    /// Integration Tests for the MemoryCacheProvider.
    /// </summary>
    public class MemoryCacheProviderTests
    {
        /// <summary>
        /// Validates adding items to the cache.
        /// </summary>
        [Fact]
        public void AddItemToCache()
        {
            string key = "key";
            using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);
            cacheProvider.AddItem(key, "value", TimeSpan.FromMilliseconds(500));
            Assert.True(memoryCache.TryGetValue(key, out _));
        }

        /// <summary>
        /// Validates items in the cache expire.
        /// </summary>
        [Fact]
        public void AddItemToCacheExpired()
        {
            string key = "key";
            using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);
            cacheProvider.AddItem(key, "value", TimeSpan.FromMilliseconds(100));
            Thread.Sleep(101);
            string? cacheItem = cacheProvider.GetItem<string>(key);
            Assert.True(cacheItem is null);
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        [Fact]
        public void GetItemFromCache()
        {
            string key = "key";
            string value = "value";
            using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set(key, value);

            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);
            string? cacheItem = cacheProvider.GetItem<string>(key);
            Assert.True(value == cacheItem);
        }
    }
}
