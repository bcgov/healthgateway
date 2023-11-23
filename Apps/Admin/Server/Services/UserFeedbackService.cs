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
    using AutoMapper;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.MapUtils;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="feedbackDelegate">The feedback delegate to interact with the DB.</param>
    /// <param name="adminTagDelegate">The admin tag delegate to interact with the DB.</param>
    /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
    /// <param name="autoMapper">The injected automapper provider.</param>
    public class UserFeedbackService(
        ILogger<UserFeedbackService> logger,
        IFeedbackDelegate feedbackDelegate,
        IAdminTagDelegate adminTagDelegate,
        IUserProfileDelegate userProfileDelegate,
        IMapper autoMapper) : IUserFeedbackService
    {
        /// <inheritdoc/>
        public RequestResult<IList<UserFeedbackView>> GetUserFeedback()
        {
            logger.LogTrace("Retrieving user feedback...");
            DbResult<IList<UserFeedback>> userFeedbackResult = feedbackDelegate.GetAllUserFeedbackEntries();

            logger.LogTrace("Retrieving user emails...");
            List<string> hdids = userFeedbackResult.Payload
                .Where(f => f.UserProfileId != null)
                .Select(f => f.UserProfileId!)
                .Distinct()
                .ToList();
            DbResult<List<UserProfile>> userProfileResult = userProfileDelegate.GetUserProfiles(hdids);
            Dictionary<string, string?> profileEmails = userProfileResult.Payload.ToDictionary(p => p.HdId, p => p.Email);

            RequestResult<IList<UserFeedbackView>> result = new()
            {
                ResourcePayload = userFeedbackResult.Payload.Select(
                        p =>
                        {
                            string? hdid = p.UserProfileId;
                            string email = string.Empty;
                            if (hdid != null && profileEmails.TryGetValue(hdid, out string? value))
                            {
                                email = value ?? string.Empty;
                            }

                            return UserFeedbackMapUtils.ToUiModel(p, email, autoMapper);
                        })
                    .ToList(),
                ResultStatus = ResultType.Success,
                TotalResultCount = userFeedbackResult.Payload.Count,
            };

            return result;
        }

        /// <inheritdoc/>
        public RequestResult<UserFeedbackView> UpdateFeedbackReview(UserFeedbackView feedback)
        {
            logger.LogTrace("Updating user feedback...");

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            feedbackDelegate.UpdateUserFeedback(autoMapper.Map<UserFeedback>(feedback));

            DbResult<UserFeedback> userFeedbackResult = feedbackDelegate.GetUserFeedbackWithFeedbackTags(feedback.Id);
            if (userFeedbackResult.Status == DbStatusCode.Read)
            {
                string email = this.GetUserEmail(userFeedbackResult.Payload.UserProfileId);
                result.ResourcePayload = UserFeedbackMapUtils.ToUiModel(userFeedbackResult.Payload, email, autoMapper);
                result.ResultStatus = ResultType.Success;
            }

            return result;
        }

        /// <inheritdoc/>
        public RequestResult<IList<AdminTagView>> GetAllTags()
        {
            logger.LogTrace("Retrieving admin tags");
            DbResult<IEnumerable<AdminTag>> adminTags = adminTagDelegate.GetAll();

            logger.LogDebug("Finished retrieving admin tags");
            IList<AdminTagView> adminTagViews = autoMapper.Map<IList<AdminTagView>>(adminTags.Payload);
            return new RequestResult<IList<AdminTagView>>
            {
                ResourcePayload = adminTagViews,
                ResultStatus = ResultType.Success,
                TotalResultCount = adminTagViews.Count,
            };
        }

        /// <inheritdoc/>
        public RequestResult<AdminTagView> CreateTag(string tagName)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            logger.LogTrace("Creating new admin tag... {TagName}", tagName);
            DbResult<AdminTag> tagResult = adminTagDelegate.Add(new() { Name = tagName });
            if (tagResult.Status == DbStatusCode.Created)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = autoMapper.Map<AdminTagView>(tagResult.Payload);
            }
            else
            {
                retVal.ResultError = new() { ResultMessage = tagResult.Message };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public RequestResult<AdminTagView> DeleteTag(AdminTagView tag)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            logger.LogTrace("Deleting admin tag... {TagName}", tag.Name);
            DbResult<AdminTag> tagResult = adminTagDelegate.Delete(autoMapper.Map<AdminTag>(tag));
            if (tagResult.Status == DbStatusCode.Deleted)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = autoMapper.Map<AdminTagView>(tagResult.Payload);
            }
            else
            {
                retVal.ResultError = new() { ResultMessage = tagResult.Message };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public RequestResult<UserFeedbackView> AssociateFeedbackTags(Guid userFeedbackId, IList<Guid> adminTagIds)
        {
            logger.LogTrace("Adding admin tags {AdminTagIds} to feedback {Feedback}", adminTagIds, userFeedbackId.ToString());

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            DbResult<UserFeedback> userFeedbackResult = feedbackDelegate.GetUserFeedbackWithFeedbackTags(userFeedbackId);
            DbResult<IEnumerable<AdminTag>> adminTagResult = adminTagDelegate.GetAdminTags(adminTagIds);

            if (userFeedbackResult.Status == DbStatusCode.Read && adminTagResult.Status == DbStatusCode.Read)
            {
                UserFeedback userFeedback = userFeedbackResult.Payload;
                IEnumerable<AdminTag> adminTags = adminTagResult.Payload;
                IEnumerable<UserFeedbackTag> feedbackTags = adminTags.Select(t => new UserFeedbackTag { AdminTag = t, UserFeedback = userFeedback });

                userFeedback.Tags.Clear();
                foreach (UserFeedbackTag userFeedbackTag in feedbackTags)
                {
                    userFeedback.Tags.Add(userFeedbackTag);
                    logger.LogDebug(
                        "User feedback tag added for admin tag id: {AdminTagId} and user feedback id: {FeedbackTagExists}",
                        userFeedbackTag.AdminTagId,
                        userFeedbackTag.UserFeedbackId);
                }

                DbResult<UserFeedback> savedUserFeedbackResult = feedbackDelegate.UpdateUserFeedbackWithTagAssociations(userFeedback);

                if (savedUserFeedbackResult.Status == DbStatusCode.Updated)
                {
                    string email = this.GetUserEmail(userFeedback.UserProfileId);
                    result.ResourcePayload = UserFeedbackMapUtils.ToUiModel(userFeedback, email, autoMapper);
                    result.ResultStatus = ResultType.Success;
                }
                else
                {
                    result.ResultError = new RequestResultError
                    {
                        ResultMessage = savedUserFeedbackResult.Message,
                    };
                }
            }
            else
            {
                result.ResultError = new RequestResultError
                {
                    ResultMessage = userFeedbackResult.Message,
                };
            }

            return result;
        }

        private string GetUserEmail(string? hdid)
        {
            string email = string.Empty;
            if (hdid != null)
            {
                DbResult<UserProfile> userProfileResult = userProfileDelegate.GetUserProfile(hdid);
                if (userProfileResult.Status == DbStatusCode.Read)
                {
                    email = userProfileResult.Payload.Email ?? string.Empty;
                }
            }

            return email;
        }
    }
}
