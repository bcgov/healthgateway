//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performaed for GenericCache.
    /// </summary>
    public interface IGenericCacheDelegate
    {
        /// <summary>
        /// Convience method to insert a CacheObject into the DB.
        /// </summary>
        /// <param name="cacheObject">The object to cache.</param>
        /// <param name="hdid">The HDID of the user this will apply to.</param>
        /// <param name="domain">The domain area for the cache object.</param>
        /// <param name="expires">The number of minutes from now when the cache will expire.</param>
        /// <param name="commit">The string representing the type of object.</param>
        /// <returns>The GenericCache object wrapped in a DBResult.</returns>
        public DBResult<GenericCache> CacheObject(object cacheObject, string hdid, string domain, int expires, bool commit = true);

        /// <summary>
        /// Convience method to query the GenericCache and return the JSON object as type T.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="hdid">The hdid of the user.</param>
        /// <param name="domain">The string representing the type of object.</param>
        /// <returns>The Generic Cache JSON object as T or null.</returns>
        public T? GetCacheObject<T>(string hdid, string domain)
            where T : class;

        /// <summary>
        /// Gets a GenericCache object from the DB using the hdid and type.
        /// </summary>
        /// <param name="hdid">The hdid of the user.</param>
        /// <param name="typeName">The string representing the type of object.</param>
        /// <returns>The GenericCache object wrapped in a DBResult.</returns>
        public DBResult<GenericCache> GetCacheObject(string hdid, string typeName);

        /// <summary>
        /// Add the given GenericCache object.
        /// </summary>
        /// <param name="cacheObject">The GenericCache object to be added to the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        public DBResult<GenericCache> AddCacheObject(GenericCache cacheObject, bool commit = true);

        /// <summary>
        /// Update the supplied CacheObject.
        /// </summary>
        /// <param name="cacheObject">The GenericCache object to be updated in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        public DBResult<GenericCache> UpdateCacheObject(GenericCache cacheObject, bool commit = true);

        /// <summary>
        /// Deletes the supplied note.
        /// </summary>
        /// <param name="cacheObject">The GenericCache object to be deleted in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        public DBResult<GenericCache> DeleteCacheObject(GenericCache cacheObject, bool commit = true);
    }
}
