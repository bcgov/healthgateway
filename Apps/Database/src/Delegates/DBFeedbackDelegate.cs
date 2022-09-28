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
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DBFeedbackDelegate : IFeedbackDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBFeedbackDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBFeedbackDelegate(
            ILogger<DBFeedbackDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DBResult<UserFeedback> InsertUserFeedback(UserFeedback feedback)
        {
            this.logger.LogTrace($"Inserting user feedback to DB... {JsonSerializer.Serialize(feedback)}");
            DBResult<UserFeedback> result = new();
            this.dbContext.Add(feedback);
            try
            {
                this.dbContext.SaveChanges();
                result.Status = DBStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                result.Status = DBStatusCode.Error;
                result.Message = e.Message;
            }

            this.logger.LogDebug($"Finished inserting user feedback to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc/>
        public void UpdateUserFeedback(UserFeedback feedback)
        {
            this.logger.LogTrace($"Updating the user feedback in DB... {feedback}");
            this.dbContext.Update(feedback);

            // Disallow updates to UserProfileId
            this.dbContext.Entry(feedback).Property(p => p.UserProfileId).IsModified = false;

            this.dbContext.SaveChanges();

            // Reload the entry after saving to retrieve the actual UserProfileId value
            this.dbContext.Entry(feedback).Reload();

            this.logger.LogDebug($"Finished updating feedback in DB. {JsonSerializer.Serialize(feedback)}");
        }

        /// <inheritdoc/>
        public DBResult<UserFeedback> UpdateUserFeedbackWithTagAssociations(UserFeedback feedback)
        {
            this.logger.LogTrace("Updating the user feedback id {UserFeedbackId} with {NumberOfAssociations} admin tag association in DB", feedback.Id, feedback.Tags.Count);
            this.dbContext.Update(feedback);
            DBResult<UserFeedback> result = new();

            try
            {
                this.dbContext.SaveChanges();
                result.Status = DBStatusCode.Updated;
                result.Payload = feedback;
            }
            catch (DbUpdateException e)
            {
                result.Status = DBStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc/>
        public DBResult<UserFeedback> GetUserFeedback(Guid feedbackId)
        {
            this.logger.LogTrace($"Getting user feedback from DB... {feedbackId}");
            UserFeedback? feedback = this.dbContext.UserFeedback.Find(feedbackId);
            DBResult<UserFeedback> result = new();
            if (feedback != null)
            {
                result.Payload = feedback;
                result.Status = DBStatusCode.Read;
            }
            else
            {
                this.logger.LogInformation($"Unable to find feedback using ID: {feedbackId}");
                result.Status = DBStatusCode.NotFound;
            }

            this.logger.LogDebug($"Finished getting user feedback from DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<UserFeedback> GetUserFeedbackWithFeedbackTags(Guid feedbackId)
        {
            this.logger.LogTrace("Getting user feedback with associations from DB {FeedbackId}", feedbackId);
            UserFeedback? feedback = this.dbContext.UserFeedback
                .Where(f => f.Id == feedbackId)
                .Include(f => f.Tags)
                .ThenInclude(t => t.AdminTag)
                .SingleOrDefault();

            DBResult<UserFeedback> result = new();
            if (feedback != null)
            {
                result.Payload = feedback;
                result.Status = DBStatusCode.Read;
            }
            else
            {
                this.logger.LogInformation("Unable to find user feedback using ID: {FeedbackId}", feedbackId);
                result.Message = $"Unable to find user feedback using ID: {feedbackId}";
                result.Status = DBStatusCode.NotFound;
            }

            return result;
        }

        /// <inheritdoc/>
        public DBResult<IList<UserFeedback>> GetAllUserFeedbackEntries()
        {
            this.logger.LogTrace("Getting all user feedback entries");
            IList<UserFeedback> feedback = this.dbContext.UserFeedback
                .Include(f => f.Tags)
                .ThenInclude(t => t.AdminTag)
                .OrderByDescending(f => f.CreatedDateTime)
                .ToList();

            DBResult<IList<UserFeedback>> result = new()
            {
                Payload = feedback,
                Status = DBStatusCode.Read,
            };
            this.logger.LogDebug("Finished getting user feedback from DB...");
            return result;
        }
    }
}
