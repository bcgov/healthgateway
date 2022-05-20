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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Converters;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly ILogger logger;
        private readonly IFeedbackDelegate feedbackDelegate;
        private readonly IAdminTagDelegate adminTagDelegate;
        private readonly IFeedbackTagDelegate feedbackTagDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedback delegate to interact with the DB.</param>
        /// <param name="adminTagDelegate">The admin tag delegate to interact with the DB.</param>
        /// <param name="feedbackTagDelegate">The feedback tag delegate to interact with the DB.</param>
        public UserFeedbackService(ILogger<UserFeedbackService> logger, IFeedbackDelegate feedbackDelegate, IAdminTagDelegate adminTagDelegate, IFeedbackTagDelegate feedbackTagDelegate)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
            this.adminTagDelegate = adminTagDelegate;
            this.feedbackTagDelegate = feedbackTagDelegate;
        }

        /// <inheritdoc />
        public RequestResult<IList<UserFeedbackView>> GetUserFeedback()
        {
            this.logger.LogTrace("Retrieving user feedback:");
            DBResult<IList<UserFeedbackAdmin>> userFeedbackResult = this.feedbackDelegate.GetAllUserFeedbackEntries();
            this.logger.LogDebug($"Finished retrieving user feedback: {JsonConvert.SerializeObject(userFeedbackResult)}");
            IList<UserFeedbackView> userFeedback = userFeedbackResult.Payload.ToUiModel();
            return new RequestResult<IList<UserFeedbackView>>()
            {
                ResourcePayload = userFeedback,
                ResultStatus = ResultType.Success,
                TotalResultCount = userFeedback.Count,
            };
        }

        /// <inheritdoc />
        public bool UpdateFeedbackReview(UserFeedbackView feedback)
        {
            this.logger.LogTrace($"Updating user feedback... {JsonConvert.SerializeObject(feedback)}");

            this.feedbackDelegate.UpdateUserFeedback(feedback.ToDbModel());
            return true;
        }

        /// <inheritdoc />
        public RequestResult<IList<AdminTagView>> GetAllTags()
        {
            this.logger.LogTrace($"Retrieving admin tags");
            DBResult<IEnumerable<AdminTag>> adminTags = this.adminTagDelegate.GetAll();

            this.logger.LogDebug($"Finished retrieving admin tags: {JsonConvert.SerializeObject(adminTags)}");
            IList<AdminTagView> adminTagViews = adminTags.Payload.ToUiModel();
            return new RequestResult<IList<AdminTagView>>()
            {
                ResourcePayload = adminTagViews,
                ResultStatus = ResultType.Success,
                TotalResultCount = adminTagViews.Count,
            };
        }

        /// <inheritdoc />
        public RequestResult<AdminTagView> CreateTag(string tagName)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogTrace($"Creating new admin tag... {tagName}");
            DBResult<AdminTag> tagResult = this.adminTagDelegate.Add(new() { Name = tagName });
            if (tagResult.Status == DBStatusCode.Created)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = tagResult.Payload.ToUiModel();
            }
            else
            {
                retVal.ResultError = new() { ResultMessage = tagResult.Message };
            }

            return retVal;
        }

        /// <inheritdoc />
        public RequestResult<AdminTagView> DeleteTag(AdminTagView tag)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogTrace($"Deleting admin tag... {tag.Name}");
            DBResult<AdminTag> tagResult = this.adminTagDelegate.Delete(tag.ToDbModel());
            if (tagResult.Status == DBStatusCode.Deleted)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = tagResult.Payload.ToUiModel();
            }
            else
            {
                retVal.ResultError = new() { ResultMessage = tagResult.Message };
            }

            return retVal;
        }

        /// <inheritdoc />
        public RequestResult<UserFeedbackTagView> AssociateFeedbackTag(Guid userFeedbackId, AdminTagView tag)
        {
            this.logger.LogTrace($"Adding admin tag to feedback... {tag}");

            DBResult<UserFeedbackTag> feedbackTagResult = this.feedbackTagDelegate.Add(
                new UserFeedbackTag()
                {
                    UserFeedbackId = userFeedbackId,
                    AdminTagId = tag.Id,
                });

            if (feedbackTagResult.Status == DBStatusCode.Created)
            {
                UserFeedbackTagView newFeedbackTag = feedbackTagResult.Payload.ToUiModel();
                newFeedbackTag.Tag = tag;
                return new RequestResult<UserFeedbackTagView>()
                {
                    ResourcePayload = newFeedbackTag,
                    ResultStatus = ResultType.Success,
                };
            }
            else
            {
                return new RequestResult<UserFeedbackTagView>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = feedbackTagResult.Message },
                };
            }
        }

        /// <inheritdoc />
        public bool DissociateFeedbackTag(Guid userFeedbackId, UserFeedbackTagView tag)
        {
            this.logger.LogTrace($"Removing admin tag from feedback... {tag}");

            DBResult<UserFeedbackTag> feedbackTagResult = this.feedbackTagDelegate.Delete(tag.ToDbModel());

            return feedbackTagResult.Status == DBStatusCode.Deleted;
        }
    }
}
