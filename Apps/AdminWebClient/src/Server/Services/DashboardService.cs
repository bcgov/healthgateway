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
    using HealthGateway.Database.Delegates;

    /// <inheritdoc />
    public class DashboardService : IDashboardService
    {
        private readonly IProfileDelegate userProfileDelegate;
        private readonly IBetaRequestDelegate betaRequestDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="betaRequestDelegate">The beta request delegate to interact with the DB.</param>
        public DashboardService(
            IProfileDelegate userProfileDelegate,
            IBetaRequestDelegate betaRequestDelegate)
        {
            this.userProfileDelegate = userProfileDelegate;
            this.betaRequestDelegate = betaRequestDelegate;
        }

        /// <inheritdoc />
        public int GetRegisteredUserCount()
        {
            return this.userProfileDelegate.GetRegisteredUsersCount();
        }

        /// <inheritdoc />
        public int GetLoggedInUsersCount()
        {
            return this.userProfileDelegate.GetLoggedInUsersCount();
        }

        /// <inheritdoc />
        public int GetWaitlistUserCount()
        {
            return this.betaRequestDelegate.GetWaitlistCount();
        }
    }
}