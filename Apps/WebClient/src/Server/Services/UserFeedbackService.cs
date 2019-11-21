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
namespace HealthGateway.WebClient.Services
{
    using System.Diagnostics.Contracts;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly ILogger logger;
        private readonly IFeedbackDelegate feedbackDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="feedbackDelegate">The feedback delegate to perform the work.</param>
        public UserFeedbackService(ILogger<UserFeedbackService> logger, IFeedbackDelegate feedbackDelegate)
        {
            this.logger = logger;
            this.feedbackDelegate = feedbackDelegate;
        }

        /// <inheritdoc />
        public DBResult<UserFeedback> CreateUserFeedback(UserFeedback userFeedback)
        {
            Contract.Requires(userFeedback != null);
            this.logger.LogDebug($"Creating user feedback... {userFeedback.IsSatisfied}");
            DBResult<UserFeedback> retVal = this.feedbackDelegate.CreateUserFeedback(userFeedback);
            this.logger.LogDebug($"Finished creating user feedback. {retVal.Status.ToString()}");

            return retVal;
        }
    }
}
