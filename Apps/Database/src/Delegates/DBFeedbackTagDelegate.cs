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

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DBFeedbackTagDelegate : IFeedbackTagDelegate
    {
        private readonly ILogger<DBFeedbackTagDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBFeedbackTagDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBFeedbackTagDelegate(
            ILogger<DBFeedbackTagDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DBResult<UserFeedbackTag> Add(UserFeedbackTag feedbackTag, bool commit = true)
        {
            this.logger.LogTrace("Adding UserFeedbackTag to DB...");
            DBResult<UserFeedbackTag> result = new()
            {
                Payload = feedbackTag,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.UserFeedbackTag.Add(feedbackTag);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to save UserFeedbackTag to DB {e}");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished adding UserFeedbackTag to DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<UserFeedbackTag> Delete(UserFeedbackTag feedbackTag, bool commit = true)
        {
            this.logger.LogTrace("Deleting UserFeedbackTag from DB...");
            DBResult<UserFeedbackTag> result = new()
            {
                Payload = feedbackTag,
                Status = DBStatusCode.Deferred,
            };

            this.dbContext.UserFeedbackTag.Remove(feedbackTag);
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

            this.logger.LogDebug("Finished deleting UserFeedbackTag in DB");
            return result;
        }

        /// <inheritdoc/>
        public DBResult<IEnumerable<UserFeedbackTag>> GetUserFeedbackTagsByFeedbackId(Guid feedbackId)
        {
            this.logger.LogTrace("Getting user feedback tags from DB for Feedback Id: {FeedbackId}", feedbackId.ToString());
            DBResult<IEnumerable<UserFeedbackTag>> result = new();
            result.Payload = this.dbContext.UserFeedbackTag.Where(t => t.UserFeedbackId == feedbackId).ToList();
            result.Status = DBStatusCode.Read;
            return result;
        }
    }
}
