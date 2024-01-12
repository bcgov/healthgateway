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
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Service to interact with the Note Delegate.
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// Creates a note in the backend.
        /// </summary>
        /// <param name="userNote">The note to create.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A note wrapped in a RequestResult.</returns>
        Task<RequestResult<UserNote>> CreateNoteAsync(UserNote userNote, CancellationToken ct = default);

        /// <summary>
        /// Gets all the notes for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="page">The page of data to fetch indexed from 0.</param>
        /// <param name="pageSize">The amount of records per page.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A List of notes wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<UserNote>>> GetNotesAsync(string hdId, int page = 0, int pageSize = 500, CancellationToken ct = default);

        /// <summary>
        /// Updates the given note in the backend.
        /// Any changes to HDID will be ignored.
        /// </summary>
        /// <param name="userNote">The note to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated Note.</returns>
        Task<RequestResult<UserNote>> UpdateNoteAsync(UserNote userNote, CancellationToken ct = default);

        /// <summary>
        /// Deletes the given note from the backend.
        /// </summary>
        /// <param name="userNote">The note to delete.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The deleted note wrapped in a RequestResult.</returns>
        Task<RequestResult<UserNote>> DeleteNoteAsync(UserNote userNote, CancellationToken ct = default);
    }
}
