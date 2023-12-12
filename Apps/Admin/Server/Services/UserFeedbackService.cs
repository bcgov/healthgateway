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
    using System.Threading;
    using System.Threading.Tasks;
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
        public async Task<RequestResult<IList<UserFeedbackView>>> GetUserFeedbackAsync(CancellationToken ct = default)
        {
            logger.LogTrace("Retrieving user feedback...");
            IList<UserFeedback> userFeedback = await feedbackDelegate.GetAllUserFeedbackEntriesAsync(ct: ct);

            logger.LogTrace("Retrieving user emails...");
            List<string> hdids = userFeedback
                .Where(f => f.UserProfileId != null)
                .Select(f => f.UserProfileId!)
                .Distinct()
                .ToList();
            IList<UserProfile> userProfiles = await userProfileDelegate.GetUserProfilesAsync(hdids, ct);
            Dictionary<string, string?> profileEmails = userProfiles.ToDictionary(p => p.HdId, p => p.Email);

            RequestResult<IList<UserFeedbackView>> result = new()
            {
                ResourcePayload = userFeedback.Select(
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
                TotalResultCount = userFeedback.Count,
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserFeedbackView>> UpdateFeedbackReviewAsync(UserFeedbackView feedback, CancellationToken ct = default)
        {
            logger.LogTrace("Updating user feedback...");

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            feedbackDelegate.UpdateUserFeedback(autoMapper.Map<UserFeedback>(feedback));

            DbResult<UserFeedback> userFeedbackResult = await feedbackDelegate.GetUserFeedbackWithFeedbackTagsAsync(feedback.Id, ct);
            if (userFeedbackResult.Status == DbStatusCode.Read)
            {
                string email = await this.GetUserEmail(userFeedbackResult.Payload.UserProfileId);
                result.ResourcePayload = UserFeedbackMapUtils.ToUiModel(userFeedbackResult.Payload, email, autoMapper);
                result.ResultStatus = ResultType.Success;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IList<AdminTagView>>> GetAllTagsAsync(CancellationToken ct = default)
        {
            logger.LogTrace("Retrieving admin tags");
            IEnumerable<AdminTag> adminTags = await adminTagDelegate.GetAllAsync(ct);

            logger.LogDebug("Finished retrieving admin tags");
            IList<AdminTagView> adminTagViews = autoMapper.Map<IList<AdminTagView>>(adminTags);
            return new RequestResult<IList<AdminTagView>>
            {
                ResourcePayload = adminTagViews,
                ResultStatus = ResultType.Success,
                TotalResultCount = adminTagViews.Count,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<AdminTagView>> CreateTagAsync(string tagName, CancellationToken ct = default)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            logger.LogTrace("Creating new admin tag... {TagName}", tagName);
            DbResult<AdminTag> tagResult = await adminTagDelegate.AddAsync(new() { Name = tagName }, true, ct);
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
        public async Task<RequestResult<AdminTagView>> DeleteTagAsync(AdminTagView tag, CancellationToken ct = default)
        {
            RequestResult<AdminTagView> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            logger.LogTrace("Deleting admin tag... {TagName}", tag.Name);
            DbResult<AdminTag> tagResult = await adminTagDelegate.DeleteAsync(autoMapper.Map<AdminTag>(tag), true, ct);
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
        public async Task<RequestResult<UserFeedbackView>> AssociateFeedbackTagsAsync(Guid userFeedbackId, IList<Guid> adminTagIds, CancellationToken ct = default)
        {
            logger.LogTrace("Adding admin tags {AdminTagIds} to feedback {Feedback}", adminTagIds, userFeedbackId.ToString());

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            DbResult<UserFeedback> userFeedbackResult = await feedbackDelegate.GetUserFeedbackWithFeedbackTagsAsync(userFeedbackId, ct);
            DbResult<IEnumerable<AdminTag>> adminTagResult = await adminTagDelegate.GetAdminTagsAsync(adminTagIds, ct);

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
                    string email = await this.GetUserEmail(userFeedback.UserProfileId);
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

        private async Task<string> GetUserEmail(string? hdid)
        {
            string email = string.Empty;
            if (hdid != null)
            {
                UserProfile? userProfile = await userProfileDelegate.GetUserProfileAsync(hdid);
                if (userProfile != null)
                {
                    email = userProfile.Email ?? string.Empty;
                }
            }

            return email;
        }
    }
}
