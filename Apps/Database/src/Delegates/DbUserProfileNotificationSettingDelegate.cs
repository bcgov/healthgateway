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
namespace HealthGateway.Database.Delegates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc/>
    /// <param name="dbContext">he context to be used when accessing the database.</param>
    public class DbUserProfileNotificationSettingDelegate(GatewayDbContext dbContext) : IUserProfileNotificationSettingDelegate
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<UserProfileNotificationSetting>>
            GetAsync(string hdid, CancellationToken ct = default)
        {
            return await dbContext.UserProfileNotificationSetting
                .Where(d => d.Hdid == hdid)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(UserProfileNotificationSetting notificationSetting, CancellationToken ct = default)
        {
            if (notificationSetting.Version == 0)
            {
                dbContext.UserProfileNotificationSetting.Add(notificationSetting);
            }
            else
            {
                dbContext.UserProfileNotificationSetting.Update(notificationSetting);
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
