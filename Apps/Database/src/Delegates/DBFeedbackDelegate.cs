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
    using System.Diagnostics.Contracts;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc />
    public class DBFeedbackDelegate : IFeedbackDelegate
    {
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBFeedbackDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBFeedbackDelegate(GatewayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> CreateUserFeedback(UserFeedback feedback)
        {
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

            return result;
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> UpdateUserFeedback(UserFeedback feedback)
        {
            Contract.Requires(feedback != null);
            DBResult<UserFeedback> result = this.GetUserFeedback(feedback.Id);
            if (result.Status == DBStatusCode.Read)
            {
                // Copy certain attributes into the fetched User Feedback
                result.Payload.Comment = feedback.Comment;
                result.Payload.UpdatedBy = feedback.UpdatedBy;
                result.Payload.Version = feedback.Version;
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

            return result;
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> GetUserFeedback(Guid feedbackId)
        {
            UserFeedback feedback = this.dbContext.UserFeedback.Find(feedbackId);
            DBResult<UserFeedback> result = new DBResult<UserFeedback>();
            result.Payload = feedback;
            result.Status = feedback != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }
    }
}
