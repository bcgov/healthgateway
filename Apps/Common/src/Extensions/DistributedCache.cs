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

namespace Microsoft.Extensions.Caching.Distributed
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions for distributed  cache
    /// </summary>
    public static class IDistributedCacheExtensions
    {
        internal static string CachePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Reads through cache - sets the object if cache doesn't have the key
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="cache">The distibuted cache instance</param>
        /// <param name="key">The key in the cache</param>
        /// <param name="factory">Factory method to create the value in case of cache miss</param>
        /// <param name="expiry">Duration of cache</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The cached object</returns>
        public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T?>> factory, TimeSpan? expiry, CancellationToken ct = default)
        {
            var obj = await cache.GetAsync<T>(key, ct);
            if (obj == null)
            {
                obj = await factory();
                await cache.SetAsync(key, obj, expiry, ct);
            }

            return obj;
        }

        /// <summary>
        /// Gets key from the cache
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="cache">The distibuted cache instance</param>
        /// <param name="key">The key in the cache</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The  cached value</returns>
        public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken ct = default)
        {
            key = CacheKey(key);
            return Deserialize<T>(await cache.GetAsync(key, ct) ?? Array.Empty<byte>());
        }

        /// <summary>
        /// Sets a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="cache">The distibuted cache instance</param>
        /// <param name="key">The key in the cache</param>
        /// <param name="obj">The value to cache</param>
        /// <param name="expiry">Duration of cache</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T obj, TimeSpan? expiry, CancellationToken ct = default)
        {
            key = CacheKey(key);
            await cache.SetAsync(key, Serialize(obj), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }, ct);
        }

        private static string CacheKey(string key) => $"{CachePrefix}{key}";

        private static T? Deserialize<T>(byte[] data) => data == null || data.Length == 0 ? default(T?) : JsonSerializer.Deserialize<T?>(data);

        private static byte[] Serialize<T>(T obj) => obj == null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(obj);
    }
}
