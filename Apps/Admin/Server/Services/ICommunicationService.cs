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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that interacts with the Communications database.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Adds a Communication to the database.
        /// </summary>
        /// <param name="communication">The communication to add to the backend.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns the added communication wrapped in a RequestResult.</returns>
        Task<RequestResult<Communication>> AddAsync(Communication communication, CancellationToken ct = default);

        /// <summary>
        /// Updates a Communication to the database.
        /// </summary>
        /// <param name="communication">The communication to update to the backend.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns the updated communication wrapped in a RequestResult.</returns>
        Task<RequestResult<Communication>> UpdateAsync(Communication communication, CancellationToken ct = default);

        /// <summary>
        /// Gets all communication entries from the database.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns a list of all communication entries, wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<Communication>>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Deletes the given communication from the backend.
        /// </summary>
        /// <param name="communication">The communication to delete.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The deleted communication wrapped in a RequestResult.</returns>
        Task<RequestResult<Communication>> DeleteAsync(Communication communication, CancellationToken ct = default);
    }
}
