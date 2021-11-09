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
    public class DBNoteDelegate : INoteDelegate
    {
        private readonly ILogger<DBNoteDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBNoteDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBNoteDelegate(
            ILogger<DBNoteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<Note> GetNote(Guid noteId, string hdid)
        {
            DBResult<Note> result = new DBResult<Note>()
            {
                Status = DBStatusCode.NotFound,
            };
            Note? note = this.dbContext.Note.Find(noteId, hdid);
            if (note != null)
            {
                result.Payload = note;
                result.Status = DBStatusCode.Read;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<Note>> GetNotes(string hdId, int offset = 0, int pagesize = 500)
        {
            this.logger.LogTrace($"Getting Notes for {hdId}...");
            DBResult<IEnumerable<Note>> result = new DBResult<IEnumerable<Note>>();
            result.Payload = this.dbContext.Note
                    .Where(p => p.HdId == hdId)
                    .OrderBy(o => o.JournalDateTime)
                    .Skip(offset)
                    .Take(pagesize)
                    .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc />
        public DBResult<Note> AddNote(Note note, bool commit = true)
        {
            this.logger.LogTrace($"Adding Note to DB...");
            DBResult<Note> result = new DBResult<Note>()
            {
                Payload = note,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Note.Add(note);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to save note to DB {e.ToString()}");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished adding Note in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<Note> UpdateNote(Note note, bool commit = true)
        {
            this.logger.LogTrace($"Updating Note request in DB...");
            DBResult<Note> result = new DBResult<Note>()
            {
                Payload = note,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Note.Update(note);
            this.dbContext.Entry(note).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Note in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<Note>> BatchUpdate(IEnumerable<Note> notes, bool commit = true)
        {
            this.logger.LogTrace($"Updating Note request in DB...");
            DBResult<IEnumerable<Note>> result = new DBResult<IEnumerable<Note>>()
            {
                Payload = notes,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Note.UpdateRange(notes);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Note in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<Note> DeleteNote(Note note, bool commit = true)
        {
            this.logger.LogTrace($"Deleting Note from DB...");
            DBResult<Note> result = new DBResult<Note>()
            {
                Payload = note,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Note.Remove(note);
            this.dbContext.Entry(note).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished deleting Note in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<Note>> GetAll(int page, int pageSize)
        {
            this.logger.LogTrace($"Retrieving all the notes for the page #{page} with pageSize: {pageSize}...");
            return DBDelegateHelper.GetPagedDBResult(
                this.dbContext.Note
                    .OrderBy(note => note.CreatedDateTime),
                page,
                pageSize);
        }
    }
}
