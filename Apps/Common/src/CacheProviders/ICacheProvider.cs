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

    /// <summary>
    /// Provides a mechanism to store string or JSON data in a Cache.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Retrieves an item from the cache if available.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <typeparam name="T">The return class type.</typeparam>
        /// <returns>The cache item serialized as T.</returns>
        T? GetItem<T>(string key)
            where T : class;

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache value.</param>
        /// <param name="expiry">The expiry timespan of the cache item.</param>
        /// <typeparam name="T">The class type to cache.</typeparam>
        void AddItem<T>(string key, T value, TimeSpan? expiry = null)
            where T : class;

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        void RemoveItem(string key);
    }
}
