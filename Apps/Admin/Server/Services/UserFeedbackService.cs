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
    using System.Linq;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Converters;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly ILogger logger;
        private readonly IFeedbackDelegate feedbackDelegate;
        private readonly IAdminTagDelegate adminTagDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedback delegate to interact with the DB.</param>
        /// <param name="adminTagDelegate">The admin tag delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        public UserFeedbackService(ILogger<UserFeedbackService> logger, IFeedbackDelegate feedbackDelegate, IAdminTagDelegate adminTagDelegate, IUserProfileDelegate userProfileDelegate)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
            this.adminTagDelegate = adminTagDelegate;
            this.userProfileDelegate = userProfileDelegate;
        }

        /// <inheritdoc />
        public RequestResult<IList<UserFeedbackView>> GetUserFeedback()
        {
            this.logger.LogTrace("Retrieving user feedback...");
            DBResult<IList<UserFeedback>> userFeedbackResult = this.feedbackDelegate.GetAllUserFeedbackEntries();

            this.logger.LogTrace("Retrieving user emails...");
            List<string> hdids = userFeedbackResult.Payload
                .Where(f => f.UserProfileId != null)
                .Select(f => f.UserProfileId!)
                .Distinct()
                .ToList();
            DBResult<List<UserProfile>> userProfileResult = this.userProfileDelegate.GetUserProfiles(hdids);
            Dictionary<string, string?> profileEmails = userProfileResult.Payload.ToDictionary(p => p.HdId, p => p.Email);

            RequestResult<IList<UserFeedbackView>> result = new()
            {
                ResourcePayload = userFeedbackResult.Payload.Select(p =>
                {
                    string email = profileEmails.TryGetValue(p.UserProfileId, out string? value) ? value ?? string.Empty : string.Empty;
                    return p.ToUiModel(email);
                }).ToList(),
                ResultStatus = ResultType.Success,
                TotalResultCount = userFeedbackResult.Payload.Count,
            };

            return result;
        }

        /// <inheritdoc />
        public RequestResult<UserFeedbackView> UpdateFeedbackReview(UserFeedbackView feedback)
        {
            this.logger.LogTrace($"Updating user feedback...");

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            this.feedbackDelegate.UpdateUserFeedback(feedback.ToDbModel());

            DBResult<UserFeedback> userFeedbackResult = this.feedbackDelegate.GetUserFeedbackWithFeedbackTags(feedback.Id);
            if (userFeedbackResult.Status == DBStatusCode.Read)
            {
                string email = this.GetUserEmail(userFeedbackResult.Payload.UserProfileId);
                result.ResourcePayload = userFeedbackResult.Payload.ToUiModel(email);
                result.ResultStatus = ResultType.Success;
            }

            return result;
        }

        /// <inheritdoc />
        public RequestResult<IList<AdminTagView>> GetAllTags()
        {
            this.logger.LogTrace($"Retrieving admin tags");
            DBResult<IEnumerable<AdminTag>> adminTags = this.adminTagDelegate.GetAll();

            this.logger.LogDebug($"Finished retrieving admin tags");
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
        public RequestResult<UserFeedbackView> AssociateFeedbackTags(Guid userFeedbackId, IList<Guid> adminTagIds)
        {
            this.logger.LogTrace("Adding admin tags {AdminTagIds} to feedback {Feedback}", adminTagIds, userFeedbackId.ToString());

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            DBResult<UserFeedback> userFeedbackResult = this.feedbackDelegate.GetUserFeedbackWithFeedbackTags(userFeedbackId);
            DBResult<IEnumerable<AdminTag>> adminTagResult = this.adminTagDelegate.GetAdminTags(adminTagIds);

            if (userFeedbackResult.Status == DBStatusCode.Read && adminTagResult.Status == DBStatusCode.Read)
            {
                UserFeedback userFeedback = userFeedbackResult.Payload;
                IEnumerable<AdminTag> adminTags = adminTagResult.Payload;
                IEnumerable<UserFeedbackTag> feedbackTags = adminTags.Select(t => new UserFeedbackTag { AdminTag = t, UserFeedback = userFeedback });

                userFeedback.Tags.Clear();
                foreach (UserFeedbackTag userFeedbackTag in feedbackTags)
                {
                    userFeedback.Tags.Add(userFeedbackTag);
                    this.logger.LogDebug("User feedback tag added for admin tag id: {AdminTagId} and user feedback id: {FeedbackTagExists}", userFeedbackTag.AdminTagId, userFeedbackTag.UserFeedbackId);
                }

                DBResult<UserFeedback> savedUserFeedbackResult = this.feedbackDelegate.UpdateUserFeedbackWithTagAssociations(userFeedback);

                if (savedUserFeedbackResult.Status == DBStatusCode.Updated)
                {
                    string email = this.GetUserEmail(userFeedback.UserProfileId);
                    result.ResourcePayload = userFeedback.ToUiModel(email);
                    result.ResultStatus = ResultType.Success;
                }
                else
                {
                    result.ResultError = new RequestResultError() { ResultMessage = savedUserFeedbackResult.Message };
                }
            }
            else
            {
                result.ResultError = new RequestResultError() { ResultMessage = userFeedbackResult.Message };
            }

            return result;
        }

        private string GetUserEmail(string? hdid)
        {
            string email = string.Empty;
            if (hdid != null)
            {
                DBResult<UserProfile> userProfileResult = this.userProfileDelegate.GetUserProfile(hdid);
                if (userProfileResult.Status == DBStatusCode.Read)
                {
                    email = userProfileResult.Payload.Email ?? string.Empty;
                }
            }

            return email;
        }
    }
}
