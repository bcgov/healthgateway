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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Service to interact with broadcasts.
    /// </summary>
    public interface IBroadcastService
    {
        /// <summary>
        /// Creates a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <returns>The created broadcast wrapped in a RequestResult.</returns>
        Task<RequestResult<Broadcast>> CreateBroadcastAsync(Broadcast broadcast);

        /// <summary>
        /// Retrieves all broadcasts.
        /// </summary>
        /// <returns>The collection of broadcasts wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcastsAsync();
    }
}
