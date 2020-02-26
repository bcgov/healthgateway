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
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Entity framework based implementation of the Application Settings delegate.
    /// </summary>
    public class DBApplicationSettingsDelegate : IApplicationSettingsDelegate
    {
        private readonly ILogger<DBApplicationSettingsDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBApplicationSettingsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public DBApplicationSettingsDelegate(
            ILogger<DBApplicationSettingsDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public void DeleteApplicationSetting(ApplicationSetting appSetting)
        {
            this.dbContext.Remove(appSetting);
        }

        /// <inheritdoc />
        public ApplicationSetting GetApplicationSetting(string application, string component, string key)
        {
            this.logger.LogTrace($"Getting application setting for {application}/{component}/{key} from DB...");
            ApplicationSetting retVal = this.dbContext.ApplicationSetting
                .Where(p => p.Application == application &&
                            p.Component == component &&
                            p.Key == key)
                .FirstOrDefault<ApplicationSetting>();
            this.logger.LogDebug($"Finished getting application setting for {application}/{component}/{key} from DB...");

            return retVal;
        }

        /// <inheritdoc />
        public List<ApplicationSetting> GetApplicationSettings(string application, string component)
        {
            this.logger.LogTrace($"Getting application setting for {application}/{component} from DB...");
            List<ApplicationSetting> retVal = this.dbContext.ApplicationSetting
                .Where(p => p.Application == application &&
                            p.Component == component)
                .ToList<ApplicationSetting>();
            this.logger.LogDebug($"Finished getting application setting for {application}/{component} from DB...");

            return retVal;
        }

        /// <inheritdoc />
        public void AddApplicationSetting(ApplicationSetting appSetting)
        {
            this.logger.LogTrace($"Setting {appSetting.Application}/{appSetting.Component}/{appSetting.Key} to {appSetting.Value}");
            this.dbContext.ApplicationSetting.Add(appSetting);
            this.logger.LogTrace($"Finished Setting {appSetting.Application}/{appSetting.Component}/{appSetting.Key} to {appSetting.Value}");
        }
    }
}
