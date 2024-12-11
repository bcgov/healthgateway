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
namespace HealthGateway.Database.Delegates;

using System;
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
public class DbAdminUserProfileDelegate(ILogger<DbAdminUserProfileDelegate> logger, GatewayDbContext dbContext) : IAdminUserProfileDelegate
{
    /// <inheritdoc/>
    public async Task<DbResult<AdminUserProfile>> GetAdminUserProfileAsync(string username, CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving admin user profile from DB with username {Username}", username);
        AdminUserProfile? profile = await dbContext.AdminUserProfile.SingleOrDefaultAsync(profile => profile.Username == username, ct);

        DbResult<AdminUserProfile> result = new();
        if (profile != null)
        {
            result.Payload = profile;
            result.Status = DbStatusCode.Read;
        }
        else
        {
            result.Status = DbStatusCode.NotFound;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IList<AdminUserProfile>> GetActiveAdminUserProfilesAsync(int activeDays, TimeSpan timeOffset, CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving all admin user profiles from DB active in the last {ActiveDays} day(s)", activeDays);
        return await dbContext.AdminUserProfile
            .Where(
                profile => profile.LastLoginDateTime.AddMinutes(timeOffset.TotalMinutes).Date >=
                           DateTime.UtcNow.AddMinutes(timeOffset.TotalMinutes).AddDays(-activeDays).Date)
            .OrderByDescending(profile => profile.LastLoginDateTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IList<AdminUserProfile>> GetInactiveAdminUserProfilesAsync(int inactiveDays, TimeSpan timeOffset, CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving all admin user profiles from DB that have been inactive for at least {InactiveDays} day(s)", inactiveDays);
        return await dbContext.AdminUserProfile
            .Where(
                profile =>
                    profile.LastLoginDateTime.AddMinutes(timeOffset.TotalMinutes).Date <= DateTime.UtcNow.AddMinutes(timeOffset.TotalMinutes).AddDays(-inactiveDays).Date)
            .OrderByDescending(profile => profile.LastLoginDateTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<DbResult<AdminUserProfile>> AddAsync(AdminUserProfile profile, CancellationToken ct = default)
    {
        logger.LogDebug("Adding admin user profile to DB");

        DbResult<AdminUserProfile> result = new();
        dbContext.Add(profile);

        try
        {
            await dbContext.SaveChangesAsync(ct);
            result.Payload = profile;
            result.Status = DbStatusCode.Created;
        }
        catch (DbUpdateException e)
        {
            logger.LogError(e, "Error adding admin user profile to DB");
            result.Status = DbStatusCode.Error;
            result.Message = e.Message;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<DbResult<AdminUserProfile>> UpdateAsync(AdminUserProfile profile, bool commit = true, CancellationToken ct = default)
    {
        logger.LogDebug("Updating admin user profile in DB");

        DbResult<AdminUserProfile> result = await this.GetAdminUserProfileAsync(profile.Username, ct);
        if (result.Status == DbStatusCode.Read)
        {
            // Copy certain attributes into the fetched Admin User Profile
            result.Payload!.LastLoginDateTime = profile.LastLoginDateTime;
            result.Payload!.UpdatedBy = profile.UpdatedBy;
            result.Payload!.Version = profile.Version;
            result.Status = DbStatusCode.Deferred;

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    logger.LogWarning(e, "Error updating admin user profile in DB");
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error updating admin user profile in DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }
        }

        return result;
    }
}
