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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBUserPreferenceDelegate : IUserPreferenceDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBUserPreferenceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBUserPreferenceDelegate(
            ILogger<DBProfileDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<UserPreference>> SaveUserPreferences(string hdid, IEnumerable<UserPreference> newPreferences, bool commit = true)
        {
            DBResult<IEnumerable<UserPreference>> preferences = this.GetUserPreferences(hdid);
            DBResult<IEnumerable<UserPreference>> result = new DBResult<IEnumerable<UserPreference>>();
            List<UserPreference> payload = new List<UserPreference>();
            result.Status = DBStatusCode.Deferred;
            result.Payload = payload;

            foreach (UserPreference newPreference in newPreferences)
            {
                UserPreference preference = preferences.Payload.FirstOrDefault(p => p.Preference == newPreference.Preference);
                if (preference != null)
                {
                    preference.UpdatedBy = newPreference.UpdatedBy;
                    preference.UpdatedDateTime = DateTime.UtcNow;
                    preference.Value = newPreference.Value;
                    payload.Add(preference);
                }
                else
                {
                    this.dbContext.UserPreference.Add(newPreference);
                    payload.Add(newPreference);
                }
            }

            foreach (UserPreference preference in preferences.Payload)
            {
                if (!newPreferences.Any(p => p.Preference == preference.Preference))
                {
                    this.dbContext.UserPreference.Remove(preference);
                }
            }

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to update UserPreference to DB {e}");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<UserPreference>> GetUserPreferences(string hdid)
        {
            DBResult<IEnumerable<UserPreference>> result = new DBResult<IEnumerable<UserPreference>>();
            result.Payload = this.dbContext.UserPreference
                                .Where(p => p.HdId == hdid)
                                .ToList();
            result.Status = DBStatusCode.Read;
            return result;
        }
    }
}
