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
        /// <returns>A note wrapped in a RequestResult.</returns>
        RequestResult<UserNote> CreateNote(UserNote userNote);

        /// <summary>
        /// Gets all the notes for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="page">The page of data to fetch indexed from 0.</param>
        /// <param name="pageSize">The amount of records per page.</param>
        /// <returns>A List of notes wrapped in a RequestResult.</returns>
        RequestResult<IEnumerable<UserNote>> GetNotes(string hdId, int page = 0, int pageSize = 500);

        /// <summary>
        /// Updates the given note in the backend.
        /// Any changes to HDID will be ignored.
        /// </summary>
        /// <param name="userNote">The note to update.</param>
        /// <returns>The updated Note.</returns>
        RequestResult<UserNote> UpdateNote(UserNote userNote);

        /// <summary>
        /// Deletes the given note from the backend.
        /// </summary>
        /// <param name="userNote">The note to delete.</param>
        /// <returns>The deleted note wrapped in a RequestResult.</returns>
        RequestResult<UserNote> DeleteNote(UserNote userNote);
    }
}
