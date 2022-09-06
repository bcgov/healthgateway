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
namespace HealthGateway.Common.Services
{
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to interact with the Communication Delegate.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Gets the active communication based on type from the backend.
        /// Only Banner, In-App, and Mobile values are supported.
        /// </summary>
        /// <param name="communicationType">The type of communication to retrieve.</param>
        /// <returns>The active communication wrapped in a RequestResult.</returns>
        RequestResult<Communication?> GetActiveCommunication(CommunicationType communicationType);

        /// <summary>
        /// Processes a change event from the DB for communications.
        /// </summary>
        /// <param name="changeEvent">The change event that was triggered.</param>
        void ProcessChange(BannerChangeEvent changeEvent);

        /// <summary>
        /// Removes any items that have been stored in the cache.
        /// </summary>
        void ClearCache();
    }
}
