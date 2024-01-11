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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for Notes.
    /// </summary>
    public interface INoteDelegate
    {
        /// <summary>
        /// Gets a list of notes ordered by the journal datetime for the given HdId.
        /// </summary>
        /// <param name="hdId">The users health identifier id.</param>
        /// <param name="offset">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of Notes wrapped in a DBResult.</returns>
        Task<DbResult<IList<Note>>> GetNotesAsync(string hdId, int offset = 0, int pageSize = 500, CancellationToken ct = default);

        /// <summary>
        /// Add the given note.
        /// </summary>
        /// <param name="note">The note to be added to the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        Task<DbResult<Note>> AddNoteAsync(Note note, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Update the supplied note.
        /// </summary>
        /// <param name="note">The note to be updated in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        Task<DbResult<Note>> UpdateNoteAsync(Note note, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Update the list of supplied notes.
        /// </summary>
        /// <param name="notes">The notes to be updated in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A List of Notes wrapped in a DBResult.</returns>
        Task<DbResult<IEnumerable<Note>>> BatchUpdateAsync(IEnumerable<Note> notes, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Deletes the supplied note.
        /// </summary>
        /// <param name="note">The note to be deleted in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        Task<DbResult<Note>> DeleteNoteAsync(Note note, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of all the notes ordered by the CreatedDateTime in ascending order.
        /// </summary>
        /// <param name="page">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of Notes wrapped in a DBResult.</returns>
        Task<IList<Note>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    }
}
