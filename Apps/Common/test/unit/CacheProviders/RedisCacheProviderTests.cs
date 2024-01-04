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
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Logging;
    using Moq;
    using StackExchange.Redis;
    using Testcontainers.Redis;
    using Xunit;

#pragma warning disable S2925 // "Thread.Sleep" should not be used in tests

    /// <summary>
    /// Integration Tests for the RedisCacheProvider.
    /// </summary>
    public class RedisCacheProviderTests : IAsyncLifetime
    {
        private readonly RedisContainer redisContainer = new RedisBuilder().Build();

        private IConnectionMultiplexer? connectionMultiplexer;

        /// <summary>
        /// Validates adding items to the cache.
        /// </summary>
        [Fact]
        public void AddItemToCache()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            const string key = "key";
            cacheProvider.AddItem(key, "value", TimeSpan.FromMilliseconds(5000));

            Assert.True(this.connectionMultiplexer.GetDatabase().KeyExists(key));
        }

        /// <summary>
        /// Validates adding items to the cache async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task AddItemToCacheAsync()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            const string key = "key";
            await cacheProvider.AddItemAsync(key, "value", TimeSpan.FromMilliseconds(5000));

            Assert.True(await this.connectionMultiplexer.GetDatabase().KeyExistsAsync(key));
        }

        /// <summary>
        /// Validates items in the cache expire.
        /// </summary>
        [Fact]
        public void AddItemToCacheExpired()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            const string key = "key";
            cacheProvider.AddItem(key, "value", TimeSpan.FromMilliseconds(100));
            Thread.Sleep(101);

            Assert.False(this.connectionMultiplexer.GetDatabase().KeyExists(key));
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        [Fact]
        public void GetItemFromCache()
        {
            const string key = "key";
            const string value = "value";
            this.connectionMultiplexer!.GetDatabase().StringSet(key, JsonSerializer.Serialize(value));

            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);
            string? cacheItem = cacheProvider.GetItem<string>(key);

            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates getting items from the cache Async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetItemFromCacheAsync()
        {
            const string key = "key";
            const string value = "value";
            await this.connectionMultiplexer!.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value));
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);
            string? cacheItem = await cacheProvider.GetItemAsync<string>(key);
            Assert.Equal(value, cacheItem);
        }

        /// <summary>
        /// Validates getting items from the cache.
        /// </summary>
        [Fact]
        public void RemoveItemFromCache()
        {
            const string key = "key";
            const string value = "value";
            this.connectionMultiplexer!.GetDatabase().StringSet(key, JsonSerializer.Serialize(value));

            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);
            cacheProvider.RemoveItem(key);

            Assert.False(this.connectionMultiplexer.GetDatabase().KeyExists(key));
        }

        /// <summary>
        /// Validates getting items from the cache async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RemoveItemFromCacheAsync()
        {
            const string key = "key";
            const string value = "value";
            await this.connectionMultiplexer!.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value));

            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);
            await cacheProvider.RemoveItemAsync(key);

            Assert.False(await this.connectionMultiplexer.GetDatabase().KeyExistsAsync(key));
        }

        /// <summary>
        /// Validates GetOrSet will both retrieve and set the value if it doesn't exist.
        /// </summary>
        [Fact]
        public void ShouldGetOrSet()
        {
            const string key = "key";
            const string value = "value";
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            Assert.False(this.connectionMultiplexer.GetDatabase().KeyExists(key));

            string ValueGetter()
            {
                return value;
            }

            string? cachedItem = cacheProvider.GetOrSet(key, ValueGetter);
            Assert.Equal(value, cachedItem);
            Assert.True(this.connectionMultiplexer.GetDatabase().KeyExists(key));
        }

        /// <summary>
        /// Validates GetOrSet will both retrieve and set the value if it doesn't exist async.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetOrSetAsync()
        {
            const string key = "key";
            const string value = "value";
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            Assert.False(await this.connectionMultiplexer.GetDatabase().KeyExistsAsync(key));

            Task<string> ValueGetter()
            {
                return Task.FromResult(value);
            }

            string? cachedItem = await cacheProvider.GetOrSetAsync(key, ValueGetter);
            Assert.Equal(value, cachedItem);
            Assert.True(await this.connectionMultiplexer.GetDatabase().KeyExistsAsync(key));
        }

        /// <summary>
        /// Ensures that null is returned when the cache is not available.
        /// </summary>
        [Fact]
        public void GetBadConnection()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ConnectionMultiplexer broken = ConnectionMultiplexer.Connect("localhost:9999,abortConnect=false");
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, broken);
            Assert.Throws<RedisConnectionException>(() => cacheProvider.GetItem<string>("NOTFOUND"));
        }

        /// <summary>
        /// performs initialization for the test as per IAsyncLifetime.
        /// </summary>
        /// <returns>A task representing nothing.</returns>
        public async Task InitializeAsync()
        {
            await this.redisContainer.StartAsync();
            this.connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(this.redisContainer.GetConnectionString());
        }

        /// <summary>
        /// Disposes resources for the test as per IAsyncLifetime.
        /// </summary>
        /// <returns>A task representing nothing.</returns>
        public async Task DisposeAsync()
        {
            await this.redisContainer.DisposeAsync();
        }
    }
}
