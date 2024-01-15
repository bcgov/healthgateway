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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Publisher for events related to user profile.
    /// </summary>
    public class UserProfileEventPublisher
    {
        /// <summary>
        /// Event raised when a user logs in.
        /// </summary>
        public event EventHandler<UserLoggedInEventArgs>? UserLoggedIn;

        /// <summary>
        /// Raises the <see cref="UserLoggedIn"/> event when a user logs in.
        /// </summary>
        /// <param name="userProfile">The user profile associated with the login event.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task OnUserLoggedInAsync(UserProfile userProfile, CancellationToken ct = default)
        {
            await Task.Yield();

            if (ct.IsCancellationRequested)
            {
                return;
            }

            this.UserLoggedIn?.Invoke(
                this,
                new UserLoggedInEventArgs
                {
                    UserProfile = userProfile,
                });
        }
    }
}
