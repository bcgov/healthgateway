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
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.Cacheable;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Xunit;

#pragma warning disable S2925 // "Thread.Sleep" should not be used in tests

    /// <summary>
    /// Represents the object type to cache.
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// Specifies that cache type is numeric.
        /// </summary>
        NumericType,

        /// <summary>
        /// Specifies that cache type is an object.
        /// </summary>
        ObjectType,

        /// <summary>
        /// Specifies that cache type is string.
        /// </summary>
        StringType,
    }

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
        /// <param name="cacheType">The type of value to cache.</param>
        [InlineData(CacheType.NumericType)]
        [InlineData(CacheType.ObjectType)]
        [InlineData(CacheType.StringType)]
        [Theory]
        public void GetOrSetItemFromCache(CacheType cacheType)
        {
            string key = $"key_{cacheType}";
            object value = GetDefaultItemValue(cacheType);

            Assert.Null(this.cacheProvider.GetItem<object>(key));

            object? cacheItem = this.cacheProvider.GetOrSet(key, () => value);

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

            await this.cacheProvider.AddItemAsync(key, value, TimeSpan.FromMilliseconds(5000));

            Assert.NotNull(await this.cacheProvider.GetItemAsync<string>(key));
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

            await this.cacheProvider.AddItemAsync(key, value, TimeSpan.FromMilliseconds(100));
            await Task.Delay(101);

            Assert.Null(await this.cacheProvider.GetItemAsync<string>(key));
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

            await this.cacheProvider.AddItemAsync(key, value);
            string? cacheItem = await this.cacheProvider.GetItemAsync<string>(key);

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

            await this.cacheProvider.AddItemAsync(key, value);
            string? cacheItem = await this.cacheProvider.GetItemAsync<string>(key);
            Assert.NotNull(cacheItem);

            await this.cacheProvider.RemoveItemAsync(key);
            cacheItem = await this.cacheProvider.GetItemAsync<string>(key);
            Assert.Null(cacheItem);
        }

        /// <summary>
        /// Validates getting or setting items from the cache.
        /// </summary>
        /// <param name="cacheType">The type of value to cache.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(CacheType.NumericType)]
        [InlineData(CacheType.ObjectType)]
        [InlineData(CacheType.StringType)]
        [Theory]
        public async Task GetOrSetItemAsyncFromCache(CacheType cacheType)
        {
            string key = $"key_{cacheType}";
            object value = GetDefaultItemValue(cacheType);

            Assert.Null(await this.cacheProvider.GetItemAsync<object>(key));

            object? cacheItem = await this.cacheProvider.GetOrSetAsync(key, () => Task.FromResult(value));

            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates complex object serialization.
        /// </summary>
        [Fact]
        public void CanCacheComplexObject()
        {
            string key = $"key_{GenerateRandomString()}";
            IHash expected = new HmacHash
            {
                Hash = "1234",
                Salt = "5678",
            };
            this.cacheProvider.AddItem(key, expected, TimeSpan.FromSeconds(30));

            IHash? actual = this.cacheProvider.GetItem<HmacHash>(key);
            Assert.NotNull(actual);
            Assert.Equal(expected.Hash, actual.Hash);
            Assert.Equal(((HmacHash)expected).Salt, ((HmacHash)actual).Salt);
        }

        private static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString()[..5];
        }

        private static object GetDefaultItemValue(CacheType cacheType)
        {
            switch (cacheType)
            {
                case CacheType.NumericType:
                    byte[] number = new byte[8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(number);
                    }

                    return number;

                case CacheType.ObjectType:
                    IEnumerable<DataSource> dataSources = [DataSource.Immunization];
                    return dataSources;

                case CacheType.StringType:
                default:
                    return GenerateRandomString();
            }
        }
    }
}
