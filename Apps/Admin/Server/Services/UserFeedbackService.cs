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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
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
    /// <param name="mappingService">The injected mapping service.</param>
    public class UserFeedbackService(
        ILogger<UserFeedbackService> logger,
        IFeedbackDelegate feedbackDelegate,
        IAdminTagDelegate adminTagDelegate,
        IUserProfileDelegate userProfileDelegate,
        IAdminServerMappingService mappingService) : IUserFeedbackService
    {
        /// <inheritdoc/>
        public async Task<RequestResult<IList<UserFeedbackView>>> GetUserFeedbackAsync(CancellationToken ct = default)
        {
            IList<UserFeedback> userFeedback = await feedbackDelegate.GetAllUserFeedbackEntriesAsync(ct: ct);

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

                            return mappingService.MapToUserFeedbackView(p, email);
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
            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            await feedbackDelegate.UpdateUserFeedbackAsync(mappingService.MapToUserFeedback(feedback), ct);

            DbResult<UserFeedback> userFeedbackResult = await feedbackDelegate.GetUserFeedbackWithFeedbackTagsAsync(feedback.Id, ct);
            if (userFeedbackResult.Status == DbStatusCode.Read)
            {
                string email = await this.GetUserEmailAsync(userFeedbackResult.Payload.UserProfileId, ct);
                result.ResourcePayload = mappingService.MapToUserFeedbackView(userFeedbackResult.Payload, email);
                result.ResultStatus = ResultType.Success;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IList<AdminTagView>>> GetAllTagsAsync(CancellationToken ct = default)
        {
            IEnumerable<AdminTag> adminTags = await adminTagDelegate.GetAllAsync(ct);

            List<AdminTagView> adminTagViews = adminTags.Select(mappingService.MapToAdminTagView).ToList();

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

            DbResult<AdminTag> tagResult = await adminTagDelegate.AddAsync(new() { Name = tagName }, true, ct);
            if (tagResult.Status == DbStatusCode.Created)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = mappingService.MapToAdminTagView(tagResult.Payload);
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

            DbResult<AdminTag> tagResult = await adminTagDelegate.DeleteAsync(mappingService.MapToAdminTag(tag), true, ct);
            if (tagResult.Status == DbStatusCode.Deleted)
            {
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = mappingService.MapToAdminTagView(tagResult.Payload);
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
            logger.LogDebug("Associating feedback with admin tag IDs {AdminTagIds}", adminTagIds);

            RequestResult<UserFeedbackView> result = new()
            {
                ResultStatus = ResultType.Error,
            };

            DbResult<UserFeedback> userFeedbackResult = await feedbackDelegate.GetUserFeedbackWithFeedbackTagsAsync(userFeedbackId, ct);
            DbResult<IEnumerable<AdminTag>> adminTagResult = await adminTagDelegate.GetAdminTagsAsync(adminTagIds, ct);

            if (userFeedbackResult.Status == DbStatusCode.Read && adminTagResult.Status == DbStatusCode.Read)
            {
                UserFeedback userFeedback = userFeedbackResult.Payload;

                userFeedback.Tags = adminTagResult.Payload.Select(t => new UserFeedbackTag { AdminTag = t, UserFeedback = userFeedback }).ToList();
                DbResult<UserFeedback> savedUserFeedbackResult = await feedbackDelegate.UpdateUserFeedbackWithTagAssociationsAsync(userFeedback, ct);

                if (savedUserFeedbackResult.Status == DbStatusCode.Updated)
                {
                    string email = await this.GetUserEmailAsync(userFeedback.UserProfileId, ct);
                    result.ResourcePayload = mappingService.MapToUserFeedbackView(savedUserFeedbackResult.Payload, email);
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

        private async Task<string> GetUserEmailAsync(string? hdid, CancellationToken ct)
        {
            string email = string.Empty;
            if (hdid != null)
            {
                UserProfile? userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct);
                if (userProfile != null)
                {
                    email = userProfile.Email ?? string.Empty;
                }
            }

            return email;
        }
    }
}
