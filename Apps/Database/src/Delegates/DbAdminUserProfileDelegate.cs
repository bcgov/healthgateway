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

using HealthGateway.Database.Constants;
using HealthGateway.Database.Context;
using HealthGateway.Database.Models;
using HealthGateway.Database.Wrapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Collections.Generic;
using System;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public class DbAdminUserProfileDelegate : IAdminUserProfileDelegate
{
    private readonly ILogger logger;
    private readonly GatewayDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbAdminUserProfileDelegate"/> class.
    /// </summary>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    public DbAdminUserProfileDelegate(
        ILogger<DbAdminUserProfileDelegate> logger,
        GatewayDbContext dbContext)
    {
        this.logger = logger;
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public DBResult<AdminUserProfile> GetAdminUserProfile(string username)
    {
        this.logger.LogTrace("Getting admin user profile from DB with Username: {Username}", username);
        DBResult<AdminUserProfile> result = new DBResult<AdminUserProfile>();
        AdminUserProfile? profile = this.dbContext.AdminUserProfile.SingleOrDefault(profile => profile.Username == username);

        if (profile != null)
        {
            result.Payload = profile;
            result.Status = DBStatusCode.Read;
        }
        else
        {
            this.logger.LogInformation("Unable to find Admin User by Username: {Username}", username);
            result.Status = DBStatusCode.NotFound;
        }

        this.logger.LogTrace("Finished getting admin user profile from DB... {Result}", JsonSerializer.Serialize(result));
        return result;
    }

    /// <inheritdoc />
    public DBResult<IEnumerable<AdminUserProfile>> GetActiveAdminUserProfiles(int activeDays, TimeSpan timeOffset)
    {
        this.logger.LogTrace("Retrieving all the active admin user profiles since {ActiveDays} day(s) ago...", activeDays);

        DBResult<IEnumerable<AdminUserProfile>> result = new DBResult<IEnumerable<AdminUserProfile>>()
        {
            Payload = this.dbContext.AdminUserProfile
                .Where(profile => profile.LastLoginDateTime.AddMinutes(timeOffset.TotalMinutes).Date >= DateTime.UtcNow.AddMinutes(timeOffset.TotalMinutes).AddDays(-activeDays).Date)
                .OrderByDescending(profile => profile.LastLoginDateTime)
                .ToList(),
            Status = DBStatusCode.Read,
        };

        this.logger.LogTrace("Finished retrieving {Count} active admin user profiles since {ActiveDays} day(s) ago...", result.Payload.Count(), activeDays);
        return result;
    }

    /// <inheritdoc />
    public DBResult<IEnumerable<AdminUserProfile>> GetInactiveAdminUserProfiles(int inactiveDays, TimeSpan timeOffset)
    {
        this.logger.LogTrace("Retrieving all the inactive admin user profiles for the past {InactiveDays} day(s)...", inactiveDays);

        DBResult<IEnumerable<AdminUserProfile>> result = new DBResult<IEnumerable<AdminUserProfile>>()
        {
            Payload = this.dbContext.AdminUserProfile
                .Where(profile => profile.LastLoginDateTime.AddMinutes(timeOffset.TotalMinutes).Date <= DateTime.UtcNow.AddMinutes(timeOffset.TotalMinutes).AddDays(-inactiveDays).Date)
                .OrderByDescending(profile => profile.LastLoginDateTime)
                .ToList(),
            Status = DBStatusCode.Read,
        };

        this.logger.LogTrace("Finished retrieving {Count} inactive admin user profiles for the past {InactiveDays} day(s)...", result.Payload.Count(), inactiveDays);
        return result;
    }

    /// <inheritdoc />
    public DBResult<AdminUserProfile> Add(AdminUserProfile profile)
    {
        this.logger.LogTrace("Inserting admin user profile to DB... {Profile}", JsonSerializer.Serialize(profile));
        DBResult<AdminUserProfile> result = new DBResult<AdminUserProfile>();
        this.dbContext.Add(profile);
        try
        {
            this.dbContext.SaveChanges();
            result.Payload = profile;
            result.Status = DBStatusCode.Created;
        }
        catch (DbUpdateException e)
        {
            result.Status = DBStatusCode.Error;
            result.Message = e.Message;
        }

        this.logger.LogTrace("Finished adding admin user profile to DB... {Result}", JsonSerializer.Serialize(result));
        return result;
    }

    /// <inheritdoc />
    public DBResult<AdminUserProfile> Update(AdminUserProfile profile, bool commit = true)
    {
        this.logger.LogTrace("Updating admin user profile in DB... {Profile}", JsonSerializer.Serialize(profile));
        DBResult<AdminUserProfile> result = this.GetAdminUserProfile(profile.Username);
        if (result.Status == DBStatusCode.Read)
        {
            // Copy certain attributes into the fetched Admin User Profile
            result.Payload.LastLoginDateTime = profile.LastLoginDateTime;
            result.Payload.UpdatedBy = profile.UpdatedBy;
            result.Payload.Version = profile.Version;
            result.Status = DBStatusCode.Deferred;

            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    this.logger.LogError(e, "Unable to update admin user profile to DB...");
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError(e, "Unable to update admin user profile to DB...");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }
        }

        this.logger.LogTrace("Finished updating admin user profile in DB... {Result}", JsonSerializer.Serialize(result));
        return result;
    }
}
