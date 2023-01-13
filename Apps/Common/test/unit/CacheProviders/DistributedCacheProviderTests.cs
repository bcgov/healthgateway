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
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Xunit;

    /// <summary>
    /// Integration Tests for the RedisCacheProvider.
    /// </summary>
    public class DistributedCacheProviderTests
    {
        private readonly ICacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheProviderTests"/> class.
        /// </summary>
        public DistributedCacheProviderTests()
        {
            this.cacheProvider = new DistributedCacheProvider(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        }

        /// <summary>
        /// Validates adding items to the cache.
        /// </summary>
        [Fact]
        public void AddItemToCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            this.cacheProvider.AddItem(key, value, TimeSpan.FromMilliseconds(5000));

            Assert.NotNull(this.cacheProvider.GetItem<string>(key));
        }

        /// <summary>
        /// Validates items in the cache expire.
        /// </summary>
        [Fact]
        public void AddItemToCacheExpired()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            this.cacheProvider.AddItem(key, value, TimeSpan.FromMilliseconds(100));
            Thread.Sleep(101);

            Assert.Null(this.cacheProvider.GetItem<string>(key));
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        [Fact]
        public void GetItemFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            this.cacheProvider.AddItem(key, value);
            string? cacheItem = this.cacheProvider.GetItem<string>(key);

            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        [Fact]
        public void RemoveItemFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            this.cacheProvider.AddItem(key, value);
            string? cacheItem = this.cacheProvider.GetItem<string>(key);
            Assert.NotNull(cacheItem);

            this.cacheProvider.RemoveItem(key);
            cacheItem = this.cacheProvider.GetItem<string>(key);
            Assert.Null(cacheItem);
        }

        /// <summary>
        /// Validates getting or setting items from the cache.
        /// </summary>
        [Fact]
        public void GetOrSetItemFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            Assert.Null(this.cacheProvider.GetItem<string>(key));

            string? cacheItem = this.cacheProvider.GetOrSet(key, () => value);

            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates adding items to the cache.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task AddItemAsyncToCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            await this.cacheProvider.AddItemAsync(key, value, TimeSpan.FromMilliseconds(5000)).ConfigureAwait(true);

            Assert.NotNull(await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true));
        }

        /// <summary>
        /// Validates items in the cache expire.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task AddItemAsyncToCacheExpired()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            await this.cacheProvider.AddItemAsync(key, value, TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
            Thread.Sleep(101);

            Assert.Null(await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true));
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetItemAsyncFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            await this.cacheProvider.AddItemAsync(key, value).ConfigureAwait(true);
            string? cacheItem = await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true);

            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task RemoveItemAsyncFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            await this.cacheProvider.AddItemAsync(key, value).ConfigureAwait(true);
            string? cacheItem = await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true);
            Assert.NotNull(cacheItem);

            await this.cacheProvider.RemoveItemAsync(key).ConfigureAwait(true);
            cacheItem = await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true);
            Assert.Null(cacheItem);
        }

        /// <summary>
        /// Validates getting or setting items from the cache.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetOrSetItemAsyncFromCache()
        {
            string key = $"key_{GenerateRandomString()}";
            string value = $"value_{GenerateRandomString()}";

            Assert.Null(await this.cacheProvider.GetItemAsync<string>(key).ConfigureAwait(true));

            string? cacheItem = await this.cacheProvider.GetOrSetAsync(key, () => Task.FromResult(value)).ConfigureAwait(true);

            Assert.Equal(value, cacheItem);
        }

        private static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString()[..5];
        }
    }
}
