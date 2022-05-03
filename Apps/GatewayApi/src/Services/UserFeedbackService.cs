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
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
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
        private readonly IRatingDelegate ratingDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly IBackgroundJobClient jobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedback delegate to perform the work.</param>
        /// <param name="ratingDelegate">The rating delegate to perform the work.</param>
        /// <param name="profileDelegate">The user profile delegate to use.</param>
        /// <param name="jobClient">The JobScheduler queue client.</param>
        public UserFeedbackService(ILogger<UserFeedbackService> logger, IFeedbackDelegate feedbackDelegate, IRatingDelegate ratingDelegate, IUserProfileDelegate profileDelegate, IBackgroundJobClient jobClient)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
            this.ratingDelegate = ratingDelegate;
            this.profileDelegate = profileDelegate;
            this.jobClient = jobClient;
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> CreateUserFeedback(UserFeedback userFeedback)
        {
            this.logger.LogTrace($"Creating user feedback... {JsonConvert.SerializeObject(userFeedback)}");
            DBResult<UserFeedback> retVal = this.feedbackDelegate.InsertUserFeedback(userFeedback);
            DBResult<UserProfile> dbResult = this.profileDelegate.GetUserProfile(userFeedback.UserProfileId);
            if (dbResult.Status == DBStatusCode.Read)
            {
                string? clientEmail = dbResult.Payload.Email;
                if (!string.IsNullOrWhiteSpace(clientEmail))
                {
                    this.logger.LogTrace($"Queueing Admin Feedback job");
                    ClientFeedback clientFeedback = new()
                    {
                        UserFeedbackId = userFeedback.Id,
                        Email = clientEmail,
                    };
                    this.jobClient.Enqueue<IAdminFeedbackJob>(j => j.SendEmail(clientFeedback));
                }
            }

            this.logger.LogDebug($"Finished creating user feedback. {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public RequestResult<Rating> CreateRating(Rating rating)
        {
            this.logger.LogTrace($"Creating rating... {JsonConvert.SerializeObject(rating)}");
            DBResult<Rating> dbRating = this.ratingDelegate.InsertRating(rating);
            this.logger.LogDebug($"Finished creating user feedback. {JsonConvert.SerializeObject(dbRating)}");

            RequestResult<Rating> result = new RequestResult<Rating>()
            {
                ResourcePayload = dbRating.Payload,
                ResultStatus = dbRating.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = dbRating.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }
    }
}
