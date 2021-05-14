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
    using HealthGateway.Admin.Models;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc />
    public class DashboardService : IDashboardService
    {
        private readonly IResourceDelegateDelegate dependentDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="dependentDelegate">The dependent delegate to interact with the DB.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        public DashboardService(
            IResourceDelegateDelegate dependentDelegate,
            IUserProfileDelegate userProfileDelegate)
        {
            this.dependentDelegate = dependentDelegate;
            this.userProfileDelegate = userProfileDelegate;
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyRegisteredUsersCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new TimeSpan(0, timeOffset, 0);
            return this.userProfileDelegate.GetDailyRegisteredUsersCount(ts);
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyLoggedInUsersCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new TimeSpan(0, timeOffset, 0);
            return this.userProfileDelegate.GetDailyLoggedInUsersCount(ts);
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyDependentCount(int timeOffset)
        {
            // Javascript offset is positive # of minutes if the local timezone is behind UTC, and negative if it is ahead.
            TimeSpan ts = new TimeSpan(0, timeOffset, 0);
            return this.dependentDelegate.GetDailyDependentCount(ts);
        }

        /// <inheritdoc />
        public int GetRecurrentUserCount(int days, int period)
        {
            return this.userProfileDelegate.GetRecurrentUserCount(days, period);
        }
    }
}
