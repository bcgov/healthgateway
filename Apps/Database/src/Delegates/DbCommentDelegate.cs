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

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbCommentDelegate(ILogger<DbCommentDelegate> logger, GatewayDbContext dbContext) : ICommentDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<IList<Comment>>> GetByParentEntryAsync(string hdId, string parentEntryId, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving comments from DB for user for entry {ParentEntryId}", parentEntryId);
            return new()
            {
                Payload = await dbContext.Comment
                    .Where(p => p.UserProfileId == hdId && p.ParentEntryId == parentEntryId)
                    .OrderBy(o => o.CreatedDateTime)
                    .ToListAsync(ct),
                Status = DbStatusCode.Read,
            };
        }

        /// <inheritdoc/>
        public async Task<DbResult<Comment>> AddAsync(Comment comment, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding comment to DB");

            DbResult<Comment> result = new()
            {
                Payload = comment,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Comment.Add(comment);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding comment to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            logger.LogDebug("Finished adding Comment to DB");
            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Comment>> UpdateAsync(Comment comment, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating comment in DB");

            DbResult<Comment> result = new()
            {
                Payload = comment,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Comment.Update(comment);
            dbContext.Entry(comment).Property(p => p.UserProfileId).IsModified = false;

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating comment in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<Comment>> DeleteAsync(Comment comment, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing comment from DB");

            DbResult<Comment> result = new()
            {
                Payload = comment,
                Status = DbStatusCode.Deferred,
            };

            dbContext.Comment.Remove(comment);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error removing comment from DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<IEnumerable<Comment>>> GetAllAsync(string hdId, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving comments from DB for user");
            return new()
            {
                Payload = await dbContext.Comment
                    .Where(p => p.UserProfileId == hdId)
                    .OrderBy(o => o.CreatedDateTime)
                    .ToListAsync(ct),
                Status = DbStatusCode.Read,
            };
        }

        /// <inheritdoc/>
        public async Task<IList<Comment>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving comments from the DB, page #{PageNumber} with page size {PageSize}", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(dbContext.Comment.OrderBy(comment => comment.CreatedDateTime), page, pageSize, ct);
        }
    }
}
