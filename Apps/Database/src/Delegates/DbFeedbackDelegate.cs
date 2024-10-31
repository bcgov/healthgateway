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
namespace HealthGateway.Database.Delegates
{
    using System;
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
    public class DbFeedbackDelegate(ILogger<DbFeedbackDelegate> logger, GatewayDbContext dbContext) : IFeedbackDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<UserFeedback>> InsertUserFeedbackAsync(UserFeedback feedback, CancellationToken ct = default)
        {
            logger.LogDebug("Adding user feedback to DB");
            dbContext.Add(feedback);

            DbResult<UserFeedback> result = new();

            try
            {
                await dbContext.SaveChangesAsync(ct);
                result.Status = DbStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Error adding user feedback to DB");
                result.Status = DbStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task UpdateUserFeedbackAsync(UserFeedback feedback, CancellationToken ct = default)
        {
            logger.LogDebug("Updating user feedback in DB with ID {FeedbackId}", feedback.Id);
            dbContext.Update(feedback);

            // Disallow updates to UserProfileId
            dbContext.Entry(feedback).Property(p => p.UserProfileId).IsModified = false;

            await dbContext.SaveChangesAsync(ct);

            // Reload the entry after saving to retrieve the actual UserProfileId value
            await dbContext.Entry(feedback).ReloadAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserFeedback>> UpdateUserFeedbackWithTagAssociationsAsync(UserFeedback feedback, CancellationToken ct = default)
        {
            logger.LogDebug("Updating tag associations for user feedback in DB with ID {UserFeedbackId}", feedback.Id);
            dbContext.Update(feedback);

            DbResult<UserFeedback> result = new();

            try
            {
                await dbContext.SaveChangesAsync(ct);
                result.Status = DbStatusCode.Updated;
                result.Payload = feedback;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Error updating tag associations for user feedback in DB");
                result.Status = DbStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<UserFeedback?> GetUserFeedbackAsync(Guid feedbackId, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user feedback from DB with ID {FeedbackId}", feedbackId);
            return await dbContext.UserFeedback.FindAsync([feedbackId], ct);
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserFeedback>> GetUserFeedbackWithFeedbackTagsAsync(Guid feedbackId, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user feedback and tags from DB with ID {FeedbackId}", feedbackId);
            UserFeedback? feedback = await dbContext.UserFeedback
                .Where(f => f.Id == feedbackId)
                .Include(f => f.Tags)
                .ThenInclude(t => t.AdminTag)
                .SingleOrDefaultAsync(ct);

            DbResult<UserFeedback> result = new();
            if (feedback != null)
            {
                result.Payload = feedback;
                result.Status = DbStatusCode.Read;
            }
            else
            {
                result.Message = $"Unable to find user feedback using ID: {feedbackId}";
                result.Status = DbStatusCode.NotFound;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IList<UserFeedback>> GetAllUserFeedbackEntriesAsync(bool includeUserProfile = false, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving all user feedback from DB");

            IQueryable<UserFeedback> query = dbContext.UserFeedback;
            if (includeUserProfile)
            {
                query = query.Include(f => f.UserProfile);
            }

            return await query
                .Include(f => f.Tags)
                .ThenInclude(t => t.AdminTag)
                .OrderByDescending(f => f.CreatedDateTime)
                .ToListAsync(ct);
        }
    }
}
