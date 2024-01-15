//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Events
{
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Consumer for events related to user profiles. It reacts to the user logged-in events by updating user profiles.
    /// </summary>
    public class UserProfileEventConsumer
    {
        private readonly ILogger logger;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileEventConsumer"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="userProfileEventPublisher">The publisher for user profile events.</param>
        /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
        public UserProfileEventConsumer(
            ILogger<UserProfileEventConsumer> logger,
            UserProfileEventPublisher userProfileEventPublisher,
            IUserProfileDelegate userProfileDelegate)
        {
            this.logger = logger;
            this.userProfileDelegate = userProfileDelegate;
            userProfileEventPublisher.UserLoggedIn += this.OnUserLoggedIn;
        }

        /// <summary>
        /// Handles the event when a user logs in.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments containing user profile.</param>
        public async void OnUserLoggedIn(object? sender, UserLoggedInEventArgs args)
        {
            DbResult<UserProfile> result = await this.userProfileDelegate.UpdateAsync(args.UserProfile, ct: args.CancellationToken);

            if (result.Status != DbStatusCode.Updated)
            {
                this.logger.LogError("Unable to update user profile for hdid: {Hdid} - Error: {Message}: ", args.UserProfile.HdId, result.Message);
            }
        }
    }
}
