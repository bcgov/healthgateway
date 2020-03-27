﻿// -------------------------------------------------------------------------
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
    /// <summary>
    /// Service that provides functionality to the admin dashboard.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the count of registered users.
        /// </summary>
        /// <returns>The count of user profiles that accepted the terms of service.</returns>
        int GetRegisteredUserCount();

        /// <summary>
        /// Retrieves the count of unregistered users that received an invite.
        /// </summary>
        /// <returns>The count of user profiles that received an invite but have not accepted the terms of service.</returns>
        int GetUnregisteredInvitedUserCount();

        /// <summary>
        /// Retrieves the count of logged in users in the current day.
        /// </summary>
        /// <returns>The count of logged in user.</returns>
        int GetLoggedInUsersCount();

        /// <summary>
        /// Retrieves the count of waitlisted users.
        /// </summary>
        /// <returns>The count of users waiting for an invite.</returns>
        int GetWaitlistUserCount();
    }
}