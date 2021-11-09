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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class DBProfileDelegate : IUserProfileDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBProfileDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBProfileDelegate(
            ILogger<DBProfileDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> InsertUserProfile(UserProfile profile)
        {
            this.logger.LogTrace($"Inserting user profile to DB... {JsonSerializer.Serialize(profile)}");
            DBResult<UserProfile> result = new DBResult<UserProfile>();
            this.dbContext.Add<UserProfile>(profile);
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

            this.logger.LogDebug($"Finished inserting user profile to DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> Update(UserProfile profile, bool commit = true)
        {
            this.logger.LogTrace($"Updating user profile in DB... {JsonSerializer.Serialize(profile)}");
            DBResult<UserProfile> result = this.GetUserProfile(profile.HdId);
            if (result.Status == DBStatusCode.Read)
            {
                // Copy certain attributes into the fetched User Profile
                result.Payload!.Email = profile.Email;
                result.Payload.AcceptedTermsOfService = profile.AcceptedTermsOfService;
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
                        result.Status = DBStatusCode.Concurrency;
                        result.Message = e.Message;
                    }
                    catch (DbUpdateException e)
                    {
                        this.logger.LogError($"Unable to update UserProfile to DB {e}");
                        result.Status = DBStatusCode.Error;
                        result.Message = e.Message;
                    }
                }
            }

            this.logger.LogDebug($"Finished updating user profile in DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> GetUserProfile(string hdId)
        {
            this.logger.LogTrace($"Getting user profile from DB... {hdId}");
            DBResult<UserProfile> result = new DBResult<UserProfile>();
            UserProfile? profile = this.dbContext.UserProfile.Find(hdId);
            if (profile != null)
            {
                result.Payload = profile;
                result.Status = DBStatusCode.Read;
            }
            else
            {
                this.logger.LogInformation($"Unable to find User by HDID {hdId}");
                result.Status = DBStatusCode.NotFound;
            }

            this.logger.LogDebug($"Finished getting user profile from DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<List<UserProfile>> GetAllUserProfilesAfter(DateTime filterDateTime, int page = 0, int pagesize = 500)
        {
            DBResult<List<UserProfile>> result = new DBResult<List<UserProfile>>();
            int offset = page * pagesize;
            result.Payload = this.dbContext.UserProfile
                                .Where(p => (p.LastLoginDateTime < filterDateTime) &&
                                             p.ClosedDateTime == null &&
                                             !string.IsNullOrWhiteSpace(p.Email))
                                .OrderBy(o => o.CreatedDateTime)
                                .Skip(offset)
                                .Take(pagesize)
                                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc />
        public DBResult<List<UserProfile>> GetClosedProfiles(DateTime filterDateTime, int page = 0, int pagesize = 500)
        {
            DBResult<List<UserProfile>> result = new DBResult<List<UserProfile>>();
            int offset = page * pagesize;
            result.Payload = this.dbContext.UserProfile
                                .Where(p => p.ClosedDateTime != null && p.ClosedDateTime < filterDateTime)
                                .OrderBy(o => o.ClosedDateTime)
                                .Skip(offset)
                                .Take(pagesize)
                                .ToList();
            result.Status = result.Payload != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyRegisteredUsersCount(TimeSpan offset)
        {
            Dictionary<DateTime, int> dateCount = this.dbContext.UserProfile
                            .Where(x => x.AcceptedTermsOfService)
                            .Select(x => new { x.HdId, createdDate = x.CreatedDateTime.AddMinutes(offset.TotalMinutes).Date })
                            .GroupBy(x => x.createdDate).Select(x => new { createdDate = x.Key, count = x.Count() })
                            .OrderBy(x => x.createdDate)
                            .ToDictionary(x => x.createdDate, x => x.count);

            return dateCount;
        }

        /// <inheritdoc />
        public IDictionary<DateTime, int> GetDailyLoggedInUsersCount(TimeSpan offset)
        {
            Dictionary<DateTime, int> dateCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime })
                .Union(
                    this.dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime }))
                .Select(x => new { x.HdId, lastLoginDate = x.LastLoginDateTime.AddMinutes(offset.TotalMinutes).Date })
                .Distinct()
                .GroupBy(x => x.lastLoginDate).Select(x => new { lastLoginDate = x.Key, count = x.Count() })
                .OrderBy(x => x.lastLoginDate)
                .ToDictionary(x => x.lastLoginDate, x => x.count);

            return dateCount;
        }

        /// <inheritdoc />
        public DBResult<IEnumerable<UserProfile>> GetAll(int page, int pageSize)
        {
            this.logger.LogTrace($"Retrieving all the user profiles for the page #{page} with pageSize: {pageSize}...");
            return DBDelegateHelper.GetPagedDBResult(
                this.dbContext.UserProfile
                    .OrderBy(userProfile => userProfile.CreatedDateTime),
                page,
                pageSize);
        }

        /// <inheritdoc />
        public int GetRecurrentUserCount(int dayCount, DateTime startDate, DateTime endDate, TimeSpan offset)
        {
            this.logger.LogTrace($"Retrieving recurring user count for {dayCount} days between {startDate} and {endDate}...");

            int recurrentCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime })
                .Union(
                    this.dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime }))
                .Select(x => new { x.HdId, lastLoginDate = x.LastLoginDateTime.AddMinutes(offset.TotalMinutes).Date })
                .Where(x => x.lastLoginDate >= startDate && x.lastLoginDate <= endDate)
                .Distinct()
                .GroupBy(x => x.HdId).Select(x => new { HdId = x.Key, count = x.Count() })
                .Where(x => x.count >= dayCount).Count();

            return recurrentCount;
        }
    }
}
