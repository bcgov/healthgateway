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
    using System.Runtime.InteropServices;
    using HealthGateway.Admin.Models;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc />
    public class DashboardService : IDashboardService
    {
        private readonly IProfileDelegate userProfileDelegate;
        private readonly IBetaRequestDelegate betaRequestDelegate;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService"/> class.
        /// </summary>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="betaRequestDelegate">The beta request delegate to interact with the DB.</param>
        /// <param name="config">The configuration provider.</param>
        public DashboardService(
            IProfileDelegate userProfileDelegate,
            IBetaRequestDelegate betaRequestDelegate,
            IConfiguration config)
        {
            this.userProfileDelegate = userProfileDelegate;
            this.betaRequestDelegate = betaRequestDelegate;
            this.configuration = config;
        }

        /// <inheritdoc />
        public int GetRegisteredUserCount()
        {
            return this.userProfileDelegate.GetRegisteredUsersCount();
        }

        /// <inheritdoc />
        public int GetUnregisteredInvitedUserCount()
        {
            return this.userProfileDelegate.GeUnregisteredInvitedUsersCount();
        }

        /// <inheritdoc />
        public int GetTodayLoggedInUsersCount()
        {
            AdminConfiguration config = new AdminConfiguration();
            this.configuration.GetSection("Admin").Bind(config);
            string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                config.WindowsTimeZoneId : config.UnixTimeZoneId;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Today.ToUniversalTime(), tz);
            return this.userProfileDelegate.GetLoggedInUsersCount(startDate);
        }

        /// <inheritdoc />
        public int GetWaitlistUserCount()
        {
            return this.betaRequestDelegate.GetWaitlistCount();
        }
    }
}