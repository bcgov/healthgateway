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

namespace HealthGateway.Admin.Server.MapUtils
{
    using System;
    using AutoMapper;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class AdminUserProfileMapUtils
    {
        /// <summary>
        /// Creates a UI model from a DB model.
        /// </summary>
        /// <param name="adminUserProfile">The DB model to convert.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <param name="timezone">The timezone to use.</param>
        /// <returns>The created UI model.</returns>
        public static AdminUserProfileView ToUiModel(AdminUserProfile adminUserProfile, IConfiguration configuration, IMapper mapper, TimeZoneInfo timezone)
        {
            AdminUserProfileView adminUserProfileView = mapper.Map<AdminUserProfile, AdminUserProfileView>(
                adminUserProfile,
                opts =>
                    opts.AfterMap(
                        (_, dest) => dest.LastLoginDateTime = dest.LastLoginDateTime != null
                            ? DateFormatter.ConvertDateTimeToTimezone((DateTime)dest.LastLoginDateTime, timezone)
                            : dest.LastLoginDateTime));
            return adminUserProfileView;
        }
    }
}
