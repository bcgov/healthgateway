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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The User Profile Model service.
    /// </summary>
    public interface IUserProfileModelService
    {
        /// <summary>
        /// Builds a user profile model.
        /// </summary>
        /// <param name="userProfile">The <see cref="UserProfile"/> to map to <see cref="UserProfileModel"/>.</param>
        /// <param name="userProfileHistoryRecordLimit">The number of <see cref="UserProfileHistory"/> to return./></param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A user profile model.</returns>
        Task<UserProfileModel> BuildUserProfileModelAsync(UserProfile userProfile, int userProfileHistoryRecordLimit, CancellationToken ct = default);

        /// <summary>
        /// Initializes an user profile model.
        /// </summary>
        /// <param name="hdid">The hdid associated with the <see cref="UserProfile"/>.</param>
        /// <param name="termsOfServiceId">The terms of service id associated with the <see cref="UserProfile"/>.</param>
        /// <param name="lastLoginDateTime">The last login date tine associated with the <see cref="UserProfile"/>.</param>
        /// <param name="email">The email address associated with the <see cref="UserProfile"/>.</param>
        /// <param name="yearOfBirth">The year of birth associated with the <see cref="UserProfile"/>.</param>
        /// <returns>A user profile model</returns>
        UserProfile InitializeUserProfile(string hdid, Guid termsOfServiceId, DateTime lastLoginDateTime, string? email, int? yearOfBirth);
    }
}
