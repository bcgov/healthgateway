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

    /// <summary>
    /// Entity framework based implementation of the Application Settings delegate.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DbApplicationSettingsDelegate : IApplicationSettingsDelegate
    {
        private readonly ILogger<DbApplicationSettingsDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbApplicationSettingsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public DbApplicationSettingsDelegate(
            ILogger<DbApplicationSettingsDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<ApplicationSetting?> GetApplicationSettingAsync(string application, string component, string key, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting application setting for {Application}/{Component}/{Key} from DB...", application, component, key);
            ApplicationSetting? retVal = await this.dbContext.ApplicationSetting
                .Where(
                    p => p.Application == application &&
                         p.Component == component &&
                         p.Key == key)
                .FirstOrDefaultAsync(ct);
            this.logger.LogDebug("Finished getting application setting for {Application}/{Component}/{Key} from DB...", application, component, key);

            return retVal;
        }

        /// <inheritdoc/>
        public void AddApplicationSetting(ApplicationSetting appSetting)
        {
            this.logger.LogTrace("Adding {Application}/{Component}/{Key} to {Value}", appSetting.Application, appSetting.Component, appSetting.Key, appSetting.Value);
            this.dbContext.ApplicationSetting.Add(appSetting);
            this.logger.LogTrace("Finished Adding {Application}/{Component}/{Key} to {Value}", appSetting.Application, appSetting.Component, appSetting.Key, appSetting.Value);
        }
    }
}
