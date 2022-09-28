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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Entity framework based implementation of the Comment delegate.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DBCommentDelegate : ICommentDelegate
    {
        private readonly ILogger<DBNoteDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCommentDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBCommentDelegate(
            ILogger<DBNoteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<Comment>> GetByParentEntry(string hdId, string parentEntryId)
        {
            this.logger.LogTrace("Getting Comments for user {HdId} and entry id {ParentEntryId}...", hdId, parentEntryId);
            DBResult<IEnumerable<Comment>> result = new();
            result.Payload = this.dbContext.Comment
                .Where(p => p.UserProfileId == hdId && p.ParentEntryId == parentEntryId)
                .OrderBy(o => o.CreatedDateTime)
                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc/>
        public DBResult<Comment> Add(Comment comment, bool commit = true)
        {
            this.logger.LogTrace("Adding Note to DB...");
            DBResult<Comment> result = new()
            {
                Payload = comment,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Comment.Add(comment);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to save Comment to DB {Exception}", e.ToString());
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished adding Comment to DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<Comment> Update(Comment comment, bool commit = true)
        {
            this.logger.LogTrace("Updating Comment in DB...");
            DBResult<Comment> result = new()
            {
                Payload = comment,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Comment.Update(comment);
            this.dbContext.Entry(comment).Property(p => p.UserProfileId).IsModified = false;
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

            this.logger.LogDebug("Finished updating Comment in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<Comment> Delete(Comment comment, bool commit = true)
        {
            this.logger.LogTrace("Deleting Comment from DB...");
            DBResult<Comment> result = new()
            {
                Payload = comment,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.Comment.Remove(comment);
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

            this.logger.LogDebug("Finished deleting Comment in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<Comment>> GetAll(string hdId)
        {
            this.logger.LogTrace("Getting Comments for user {HdId}...", hdId);
            DBResult<IEnumerable<Comment>> result = new();
            result.Payload = this.dbContext.Comment
                .Where(p => p.UserProfileId == hdId)
                .OrderBy(o => o.CreatedDateTime)
                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<Comment>> GetAll(int page, int pageSize)
        {
            this.logger.LogTrace("Retrieving all the comments for the page #{Page} with pageSize: {PageSize}...", page, pageSize);
            return DBDelegateHelper.GetPagedDBResult(
                this.dbContext.Comment
                    .OrderBy(comment => comment.CreatedDateTime),
                page,
                pageSize);
        }
    }
}
