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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbUserPreferenceDelegate(ILogger<DbUserPreferenceDelegate> logger, GatewayDbContext dbContext) : IUserPreferenceDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<UserPreference>> CreateUserPreferenceAsync(UserPreference userPreference, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding user preference to DB: {Preference}", userPreference.Preference);
            dbContext.UserPreference.Add(userPreference);

            DbResult<UserPreference> result = new()
            {
                Payload = userPreference,
                Status = DbStatusCode.Deferred,
            };

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error adding user preference to DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding user preference to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserPreference>> UpdateUserPreferenceAsync(UserPreference userPreference, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating user preference in DB: {Preference}", userPreference.Preference);

            dbContext.UserPreference.Update(userPreference);
            dbContext.Entry(userPreference).Property(p => p.HdId).IsModified = false;

            DbResult<UserPreference> result = new()
            {
                Payload = userPreference,
                Status = DbStatusCode.Deferred,
            };

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating user preference in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error updating user preference in DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user preferences from DB for {Hdid}", hdid);
            return await dbContext.UserPreference
                .Where(p => p.HdId == hdid)
                .ToListAsync(ct);
        }
    }
}
