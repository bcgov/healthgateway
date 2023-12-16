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
    [ExcludeFromCodeCoverage]
    public class DbUserPreferenceDelegate : IUserPreferenceDelegate
    {
        private readonly ILogger<DbUserPreferenceDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUserPreferenceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbUserPreferenceDelegate(
            ILogger<DbUserPreferenceDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserPreference>> CreateUserPreferenceAsync(UserPreference userPreference, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Creating new User Preference in DB...");
            DbResult<UserPreference> result = new()
            {
                Payload = userPreference,
                Status = DbStatusCode.Deferred,
            };
            await this.dbContext.UserPreference.AddAsync(userPreference, ct);

            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to create UserPreference to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserPreference>> UpdateUserPreferenceAsync(UserPreference userPreference, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating User Preference in DB...");
            DbResult<UserPreference> result = new()
            {
                Payload = userPreference,
                Status = DbStatusCode.Deferred,
            };
            this.dbContext.UserPreference.Update(userPreference);
            this.dbContext.Entry(userPreference).Property(p => p.HdId).IsModified = false;

            if (commit)
            {
                try
                {
                    await this.dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to update UserPreference to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default)
        {
            return await this.dbContext.UserPreference
                .Where(p => p.HdId == hdid)
                .ToListAsync(ct);
        }
    }
}
