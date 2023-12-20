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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly IFeedbackDelegate feedbackDelegate;
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger logger;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IRatingDelegate ratingDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedback delegate to perform the work.</param>
        /// <param name="ratingDelegate">The rating delegate to perform the work.</param>
        /// <param name="profileDelegate">The user profile delegate to use.</param>
        /// <param name="jobClient">The JobScheduler queue client.</param>
        public UserFeedbackService(
            ILogger<UserFeedbackService> logger,
            IFeedbackDelegate feedbackDelegate,
            IRatingDelegate ratingDelegate,
            IUserProfileDelegate profileDelegate,
            IBackgroundJobClient jobClient)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
            this.ratingDelegate = ratingDelegate;
            this.profileDelegate = profileDelegate;
            this.jobClient = jobClient;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserFeedback>> CreateUserFeedbackAsync(UserFeedback userFeedback, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating user feedback...");
            DbResult<UserFeedback> retVal = await this.feedbackDelegate.InsertUserFeedbackAsync(userFeedback, ct);
            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(userFeedback.UserProfileId, ct);
            string? clientEmail = userProfile?.Email;
            if (!string.IsNullOrWhiteSpace(clientEmail))
            {
                this.logger.LogTrace("Queueing Admin Feedback job");
                ClientFeedback clientFeedback = new()
                {
                    UserFeedbackId = userFeedback.Id,
                    Email = clientEmail,
                };
                this.jobClient.Enqueue<IAdminFeedbackJob>(j => j.SendEmail(clientFeedback));
            }

            this.logger.LogDebug("Finished creating user feedback");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Rating>> CreateRatingAsync(Rating rating, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating rating...");
            DbResult<Rating> dbRating = await this.ratingDelegate.InsertRatingAsync(rating, ct);
            this.logger.LogDebug("Finished creating user feedback");

            RequestResult<Rating> result = new()
            {
                ResourcePayload = dbRating.Payload,
                ResultStatus = dbRating.Status == DbStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = dbRating.Message,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };
            return result;
        }
    }
}
