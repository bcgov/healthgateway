﻿// -------------------------------------------------------------------------
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
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
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

        /// <inheritdoc />
        public DBResult<UserFeedback> InsertUserFeedback(UserFeedback feedback)
        {
            this.logger.LogTrace($"Inserting user feedback to DB... {JsonSerializer.Serialize(feedback)}");
            DBResult<UserFeedback> result = new DBResult<UserFeedback>();
            this.dbContext.Add<UserFeedback>(feedback);
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

        /// <inheritdoc />
        public void UpdateUserFeedback(UserFeedback feedback)
        {
            this.logger.LogTrace($"Updating the user feedback in DB... {feedback}");
            this.dbContext.Update<UserFeedback>(feedback);

            // Prevent updates to associated user profile id
            this.dbContext.Entry(feedback).Property(p => p.UserProfileId).IsModified = false;
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished updating feedback in DB. {JsonSerializer.Serialize(feedback)}");
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> GetUserFeedback(Guid feedbackId)
        {
            this.logger.LogTrace($"Getting user feedback from DB... {feedbackId}");
            UserFeedback feedback = this.dbContext.UserFeedback.Find(feedbackId);
            DBResult<UserFeedback> result = new DBResult<UserFeedback>();
            result.Payload = feedback;
            result.Status = feedback != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            this.logger.LogDebug($"Finished getting user feedback from DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<List<UserFeedbackAdmin>> GetAllUserFeedbackEntries()
        {
            this.logger.LogTrace($"Getting all user feedback entries");
            List<UserFeedbackAdmin> feedback = this.dbContext.UserFeedback
                .Select(x => new UserFeedbackAdmin
                {
                    Id = x.Id,
                    IsSatisfied = x.IsSatisfied,
                    IsReviewed = x.IsReviewed,
                    UpdatedBy = x.UpdatedBy,
                    Comment = x.Comment,
                    CreatedBy = x.CreatedBy,
                    CreatedDateTime = x.CreatedDateTime,
                    UserProfileId = x.UserProfileId,
                    UpdatedDateTime = x.UpdatedDateTime,
                    Version = x.Version,
                })
                .OrderByDescending(f => f.CreatedDateTime).ToList();

            Dictionary<string, string?> profileEmails = this.dbContext.UserProfile.Where(u => feedback.Select(f => f.UserProfileId).Contains(u.HdId)).ToDictionary(x => x.HdId, x => x.Email);

            foreach (UserFeedbackAdmin feedbackAdmin in feedback)
            {
                if (!string.IsNullOrWhiteSpace(feedbackAdmin.UserProfileId))
                {
                    string? email;
                    if (profileEmails.TryGetValue(feedbackAdmin.UserProfileId, out email))
                    {
                        feedbackAdmin.Email = email != null ? email : string.Empty;
                    }
                }
            }

            DBResult<List<UserFeedbackAdmin>> result = new DBResult<List<UserFeedbackAdmin>>();
            result.Payload = feedback;
            result.Status = feedback != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            this.logger.LogDebug($"Finished getting user feedback from DB... {JsonSerializer.Serialize(result)}");
            return result;
        }
    }
}
