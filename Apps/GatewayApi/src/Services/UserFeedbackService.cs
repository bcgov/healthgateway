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
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly IFeedbackDelegate feedbackDelegate;
        private readonly IBackgroundJobClient jobClient;
        private readonly IGatewayApiMappingService mappingService;
        private readonly IAuthenticationDelegate authenticationDelegate;
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
        /// <param name="mappingService">The injected mapping service.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        public UserFeedbackService(
            ILogger<UserFeedbackService> logger,
            IFeedbackDelegate feedbackDelegate,
            IRatingDelegate ratingDelegate,
            IUserProfileDelegate profileDelegate,
            IBackgroundJobClient jobClient,
            IGatewayApiMappingService mappingService,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
            this.ratingDelegate = ratingDelegate;
            this.profileDelegate = profileDelegate;
            this.jobClient = jobClient;
            this.mappingService = mappingService;
            this.authenticationDelegate = authenticationDelegate;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserFeedback>> CreateUserFeedbackAsync(Feedback feedback, string hdid, CancellationToken ct = default)
        {
            UserFeedback userFeedback = this.mappingService.MapToUserFeedback(feedback, hdid);
            userFeedback.ClientCode ??= this.authenticationDelegate.FetchAuthenticatedUserClientType();

            DbResult<UserFeedback> retVal = await this.feedbackDelegate.InsertUserFeedbackAsync(userFeedback, ct);
            UserProfile? userProfile = await this.profileDelegate.GetUserProfileAsync(userFeedback.UserProfileId, ct: ct);

            string? clientEmail = userProfile?.Email;
            if (!string.IsNullOrWhiteSpace(clientEmail))
            {
                ClientFeedback clientFeedback = new()
                {
                    UserFeedbackId = userFeedback.Id,
                    Email = clientEmail,
                };
                this.logger.LogDebug("Queueing job to send feedback to Health Gateway support email address");
                this.jobClient.Enqueue<IAdminFeedbackJob>(j => j.SendEmailAsync(clientFeedback, ct));
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<RatingModel>> CreateRatingAsync(SubmitRating rating, CancellationToken ct = default)
        {
            DbResult<Rating> dbRating = await this.ratingDelegate.InsertRatingAsync(this.mappingService.MapToRating(rating), ct);

            return new()
            {
                ResourcePayload = dbRating.Status == DbStatusCode.Created ? this.mappingService.MapToRatingModel(dbRating.Payload) : null,
                ResultStatus = dbRating.Status == DbStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = dbRating.Status == DbStatusCode.Error
                    ? new RequestResultError
                    {
                        ResultMessage = dbRating.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };
        }
    }
}
