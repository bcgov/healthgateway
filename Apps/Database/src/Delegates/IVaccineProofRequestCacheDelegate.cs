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
namespace HealthGateway.Database.Delegates
{
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Provides a mechanism to store and retrieve Vaccine Proof Request cache items.
    /// </summary>
    public interface IVaccineProofRequestCacheDelegate
    {
        /// <summary>
        /// Gets the most recent cache item matching the supplied criteria.
        /// </summary>
        /// <param name="personalIdentifier">the unique person identifier for this item.</param>
        /// <param name="proofTemplate">The template that is being rendered.</param>
        /// <param name="shcImageHash">The hash of the Smart Health Card image.</param>
        /// <returns>The Vaccine Proof Request Cache item or null if not found/error.</returns>
        VaccineProofRequestCache? GetCacheItem(string personalIdentifier, VaccineProofTemplate proofTemplate, string shcImageHash);

        /// <summary>
        /// Adds the cache item to the underlying store.
        /// </summary>
        /// <param name="cacheItem">The item to cache.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        void AddCacheItem(VaccineProofRequestCache cacheItem, bool commit = true);
    }
}
