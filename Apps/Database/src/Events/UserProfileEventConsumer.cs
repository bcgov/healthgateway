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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Consumer for events related to user profiles. It reacts to the user logged-in events by updating user profiles.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UserProfileEventConsumer"/> class.
    /// </remarks>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="userProfileDelegate">The profile delegate to interact with the DB.</param>
    public class UserProfileEventConsumer(ILogger<UserProfileEventConsumer> logger, IUserProfileDelegate userProfileDelegate)
    {
        /// <summary>
        /// Handles the asynchronous event when a user logs in.
        /// </summary>
        /// <param name="args">The event arguments containing user profile.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        protected async Task OnUserLoggedInAsync(UserLoggedInEventArgs args, CancellationToken ct = default)
        {
            DbResult<UserProfile> result = await userProfileDelegate.UpdateAsync(args.UserProfile, ct: ct);

            if (result.Status != DbStatusCode.Updated)
            {
                logger.LogError("Unable to update user profile for hdid: {Hdid} - Error: {Message}: ", args.UserProfile.HdId, result.Message);
            }
        }
    }
}
