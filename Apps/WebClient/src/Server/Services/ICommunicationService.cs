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
namespace HealthGateway.WebClient.Services
{
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to interact with the Communication Delegate.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Gets the active communication banner based on type from the backend.
        /// Only Banner and In-App values are supported.
        /// </summary>
        /// <param name="communicationType">The type of communication banner to retrieve.</param>
        /// <returns>The active communication wrapped in a RequestResult.</returns>
        RequestResult<Communication> GetActiveBanner(CommunicationType communicationType);

        /// <summary>
        /// Adds the associated banner to the local cache.
        /// </summary>
        /// <param name="cacheEntry">The communication to be cached..</param>
        /// <param name="cacheType">The communication type to be removed.</param>
        public void AddBannerCache(RequestResult<Communication> cacheEntry, CommunicationType cacheType);

        /// <summary>
        /// Removes the associated banner from the local cache.
        /// </summary>
        /// <param name="cacheType">The communication type to be removed.</param>
        void RemoveBannerCache(CommunicationType cacheType);

        /// <summary>
        /// Gets the associated banner from the local cache.
        /// </summary>
        /// <param name="cacheType">The communication type to be removed.</param>
        /// <returns>The cached object.</returns>
        RequestResult<Communication>? GetBannerCache(CommunicationType cacheType);
    }
}
