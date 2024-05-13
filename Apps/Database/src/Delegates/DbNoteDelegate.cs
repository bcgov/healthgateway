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
    [ExcludeFromCodeCoverage]
    public class DbNoteDelegate : INoteDelegate
    {
        private readonly ILogger<DbNoteDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbNoteDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbNoteDelegate(
            ILogger<DbNoteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<DbResult<IList<Note>>> GetNotesAsync(string hdId, int offset = 0, int pageSize = 500, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting Notes for {HdId}...", hdId);
            DbResult<IList<Note>> result = new();
            result.Payload = await this.dbContext.Note
                .Where(p => p.HdId == hdId)
                .OrderBy(o => o.JournalDate)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync(ct);
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> AddNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Adding Note to DB...");
            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Note.Add(note);
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError(e, "Unable to save note to DB {Message}", e.Message);
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished adding Note in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> UpdateNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating Note request in DB...");
            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Note.Update(note);
            this.dbContext.Entry(note).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished updating Note in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<IEnumerable<Note>>> BatchUpdateAsync(IEnumerable<Note> notes, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating Note request in DB...");
            DbResult<IEnumerable<Note>> result = new()
            {
                Payload = notes.ToList(),
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Note.UpdateRange(result.Payload);
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished updating Note in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Note>> DeleteNoteAsync(Note note, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Deleting Note from DB...");
            DbResult<Note> result = new()
            {
                Payload = note,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.Note.Remove(note);
            this.dbContext.Entry(note).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished deleting Note in DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<Note>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            this.logger.LogTrace("Retrieving all the notes for the page #{Page} with pageSize: {PageSize}...", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(this.dbContext.Note.OrderBy(note => note.CreatedDateTime), page, pageSize, ct);
        }
    }
}
