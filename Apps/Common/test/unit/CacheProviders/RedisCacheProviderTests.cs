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
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Logging;
    using Moq;
    using StackExchange.Redis;
    using Xunit;

    /// <summary>
    /// Integration Tests for the RedisCacheProvider.
    /// </summary>
    public class RedisCacheProviderTests : IAsyncLifetime
    {
        private readonly RedisTestcontainer redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(new RedisTestcontainerConfiguration("redis:latest"))
            .Build();

        private IConnectionMultiplexer? connectionMultiplexer;

        /// <summary>
        /// Validates adding items to the cache.
        /// </summary>
        [Fact]
        public void AddItemToCache()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            string key = "key";
            cacheProvider.AddItem(key, "value", TimeSpan.FromMilliseconds(5000));

            Assert.True(this.connectionMultiplexer.GetDatabase().KeyExists(key));
        }

        /// <summary>
        /// Validates items in the cache expire.
        /// </summary>
        [Fact]
        public void AddItemToCacheExpired()
        {
            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);

            string key = "key";
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
            string key = "key";
            string value = "value";
            this.connectionMultiplexer!.GetDatabase().StringSet(key, JsonSerializer.Serialize(value));

            Mock<ILogger<RedisCacheProvider>> mockLogger = new();
            ICacheProvider cacheProvider = new RedisCacheProvider(mockLogger.Object, this.connectionMultiplexer);
            string? cacheItem = cacheProvider.GetItem<string>(key);

            Assert.True(value == cacheItem);
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
            string? result = cacheProvider.GetItem<string>("NOTFOUND");
            Assert.Null(result);
        }

        /// <summary>
        /// performs initialization for the test as per IAsyncLifetime.
        /// </summary>
        /// <returns>A task representing nothing.</returns>
        public async Task InitializeAsync()
        {
            await this.redisContainer.StartAsync().ConfigureAwait(true);
            this.connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(this.redisContainer.ConnectionString).ConfigureAwait(true);
        }

        /// <summary>
        /// Disposes resources for the test as per IAsyncLifetime.
        /// </summary>
        /// <returns>A task representing nothing.</returns>
        public async Task DisposeAsync()
        {
            await this.redisContainer.DisposeAsync().ConfigureAwait(true);
        }
    }
}
