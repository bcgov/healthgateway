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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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
        public DbResult<Note> GetNote(Guid noteId, string hdid)
        {
            DbResult<Note> result = new()
            {
                Status = DbStatusCode.NotFound,
            };
            Note? note = this.dbContext.Note.Find(noteId, hdid);
            if (note != null)
            {
                result.Payload = note;
                result.Status = DbStatusCode.Read;
            }

            return result;
        }

        /// <inheritdoc/>
        public DbResult<IEnumerable<Note>> GetNotes(string hdId, int offset = 0, int pageSize = 500)
        {
            this.logger.LogTrace("Getting Notes for {HdId}...", hdId);
            DbResult<IEnumerable<Note>> result = new();
            result.Payload = this.dbContext.Note
                .Where(p => p.HdId == hdId)
                .OrderBy(o => o.JournalDate)
                .Skip(offset)
                .Take(pageSize)
                .ToList();
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public DbResult<Note> AddNote(Note note, bool commit = true)
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
                    this.dbContext.SaveChanges();
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to save note to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished adding Note in DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<Note> UpdateNote(Note note, bool commit = true)
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
                    this.dbContext.SaveChanges();
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
        public DbResult<IEnumerable<Note>> BatchUpdate(IEnumerable<Note> notes, bool commit = true)
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
                    this.dbContext.SaveChanges();
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
        public DbResult<Note> DeleteNote(Note note, bool commit = true)
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
                    this.dbContext.SaveChanges();
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
        public DbResult<IEnumerable<Note>> GetAll(int page, int pageSize)
        {
            this.logger.LogTrace("Retrieving all the notes for the page #{Page} with pageSize: {PageSize}...", page, pageSize);
            return DbDelegateHelper.GetPagedDbResult(
                this.dbContext.Note
                    .OrderBy(note => note.CreatedDateTime),
                page,
                pageSize);
        }
    }
}
