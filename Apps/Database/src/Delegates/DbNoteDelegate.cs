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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Entity framework based implementation of the Note delegate.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbNoteDelegate(ILogger<DbNoteDelegate> logger, GatewayDbContext dbContext) : INoteDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<IList<Note>>> GetNotesAsync(string hdId, int offset = 0, int pageSize = 500, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving notes for {Hdid}, offset {Offset} with page size {PageSize}", hdId, offset, pageSize);
            return new()
            {
                Payload = await dbContext.Note
                    .Where(p => p.HdId == hdId)
                    .OrderBy(o => o.JournalDate)
                    .Skip(offset)
                    .Take(pageSize)
                    .ToListAsync(ct),
                Status = DbStatusCode.Read,
            };
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> AddNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding note to DB");
            dbContext.Note.Add(note);

            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding note to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> UpdateNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating note in DB");
            dbContext.Note.Update(note);
            dbContext.Entry(note).Property(p => p.HdId).IsModified = false;

            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating note in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<IEnumerable<Note>>> BatchUpdateAsync(IEnumerable<Note> notes, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating notes in DB");

            DbResult<IEnumerable<Note>> result = new()
            {
                Payload = notes.ToList(),
                Status = DbStatusCode.Deferred,
            };

            dbContext.Note.UpdateRange(result.Payload);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating notes in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> DeleteNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing note from DB");

            dbContext.Note.Remove(note);
            dbContext.Entry(note).Property(p => p.HdId).IsModified = false;

            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error removing note from DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Note>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving notes from DB, page #{PageNumber} with page size {PageSize}", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(dbContext.Note.OrderBy(note => note.CreatedDateTime), page, pageSize, ct);
        }
    }
}
