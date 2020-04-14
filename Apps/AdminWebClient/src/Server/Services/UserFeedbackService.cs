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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedeback delegate to interact with the DB.</param>
        public UserFeedbackService(ILogger<UserFeedbackService> logger, IFeedbackDelegate feedbackDelegate)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
        }

        /// <inheritdoc />
        public RequestResult<List<UserFeedbackView>> GetUserFeedback()
        {
            this.logger.LogTrace($"Retrieving pending beta requests");
            DBResult<List<UserFeedbackAdmin>> userfeedbackResult = this.feedbackDelegate.GetAllUserFeedbackEntries();
            this.logger.LogDebug($"Finished retrieving user feedback: {JsonConvert.SerializeObject(userfeedbackResult)}");
            List<UserFeedbackView> userFeedback = UserFeedbackView.CreateListFromDbModel(userfeedbackResult.Payload);
            return new RequestResult<List<UserFeedbackView>>()
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

            // Get the requets that still need to be invited
            this.feedbackDelegate.UpdateUserFeedback(feedback.ToDbModel());
            return true;
        }
    }
}