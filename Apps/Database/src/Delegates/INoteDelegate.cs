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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Operations to be performaed for Notes.
    /// </summary>
    public interface INoteDelegate
    {
        /// <summary>
        /// Gets a note from the DB using the noteId.
        /// </summary>
        /// <param name="noteId">The Note ID to retrieve.</param>
        /// <returns>The note wrapped in a DBResult.</returns>
        public DBResult<Note> GetNote(Guid noteId);

        /// <summary>
        /// Gets a list of notes ordered by the journal datetime for the given HdId.
        /// </summary>
        /// <param name="hdId">The users health identifier id.</param>
        /// <param name="offset">The starting offset for the query.</param>
        /// <param name="pagesize">The maximum amount of rows to return.</param>
        /// <returns>A list of Notes wrapped in a DBResult.</returns>
        public DBResult<List<Note>> GetNotes(string hdId, int offset = 0, int pagesize = 500);

        /// <summary>
        /// Add the given note.
        /// </summary>
        /// <param name="note">The note to be added to the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        public DBResult<Note> AddNote(Note note, bool commit = true);

        /// <summary>
        /// Update the supplied note.
        /// </summary>
        /// <param name="note">The note to be updated in the backend.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Note wrapped in a DBResult.</returns>
        public DBResult<Note> UpdateNote(Note note, bool commit = true);
    }
}
