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
namespace HealthGateway.Database.Delegates
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database context.</param>
    [ExcludeFromCodeCoverage]
    public class DbApplicationSettingsDelegate(ILogger<DbApplicationSettingsDelegate> logger, GatewayDbContext dbContext) : IApplicationSettingsDelegate
    {
        /// <inheritdoc/>
        public async Task<ApplicationSetting?> GetApplicationSettingAsync(string application, string component, string key, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving application setting from DB for {Application}/{Component}/{Key}", application, component, key);
            ApplicationSetting? retVal = await dbContext.ApplicationSetting
                .Where(
                    p => p.Application == application &&
                         p.Component == component &&
                         p.Key == key)
                .FirstOrDefaultAsync(ct);

            return retVal;
        }

        /// <inheritdoc/>
        public void AddApplicationSetting(ApplicationSetting appSetting)
        {
            logger.LogDebug(
                "Adding application setting to DB, setting {Application}/{Component}/{Key} to {Value}",
                appSetting.Application,
                appSetting.Component,
                appSetting.Key,
                appSetting.Value);
            dbContext.ApplicationSetting.Add(appSetting);
        }
    }
}
