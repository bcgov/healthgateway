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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a mechanism to store string or JSON data in a Cache.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Key used to store cache for blocked access.
        /// </summary>
        public static readonly CompositeFormat BlockedAccessCachePrefixKey = CompositeFormat.Parse("BlockedAccess:Hdid:{0}");

        /// <summary>
        /// Retrieves an item from the cache if available.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <typeparam name="T">The return class type.</typeparam>
        /// <returns>The cache item serialized as T.</returns>
        T? GetItem<T>(string key);

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="expiry">The expiry timespan of the cache item.</param>
        /// <typeparam name="T">The class type to cache.</typeparam>
        void AddItem<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        void RemoveItem(string key);

        /// <summary>
        /// Gets or sets a value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="valueGetter">A function to generate the value to cache if cache miss.</param>
        /// <param name="expiry">The expiry timespan of the cache item.</param>
        /// <typeparam name="T">The class type to cache.</typeparam>
        /// <returns>The cache item serialized as T.</returns>
        T? GetOrSet<T>(string key, Func<T> valueGetter, TimeSpan? expiry = null);

        /// <summary>
        /// Retrieves an item from the cache if available.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <typeparam name="T">The return class type.</typeparam>
        /// <returns>The cache item serialized as T.</returns>
        Task<T?> GetItemAsync<T>(string key, CancellationToken ct = default);

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="expiry">The expiry timespan of the cache item.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <typeparam name="T">The class type to cache.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddItemAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveItemAsync(string key, CancellationToken ct = default);

        /// <summary>
        /// Gets or sets a value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="valueGetter">A function to generate the value to cache if cache miss.</param>
        /// <param name="expiry">The expiry timespan of the cache item.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <typeparam name="T">The class type to cache.</typeparam>
        /// <returns>The cache item serialized as T.</returns>
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> valueGetter, TimeSpan? expiry = null, CancellationToken ct = default);
    }
}
