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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbProfileDelegate : IUserProfileDelegate
    {
        private readonly GatewayDbContext dbContext;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbProfileDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbProfileDelegate(
            ILogger<DbProfileDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public DbResult<UserProfile> InsertUserProfile(UserProfile profile)
        {
            this.logger.LogTrace("Inserting user profile to DB...");
            DbResult<UserProfile> result = new();
            this.dbContext.Add(profile);
            try
            {
                this.dbContext.SaveChanges();
                result.Payload = profile;
                result.Status = DbStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                result.Status = DbStatusCode.Error;
                result.Message = e.Message;
            }

            this.logger.LogDebug("Finished inserting user profile to DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<UserProfile> Update(UserProfile profile, bool commit = true)
        {
            this.logger.LogTrace("Updating user profile in DB");
            DbResult<UserProfile> result = this.GetUserProfile(profile.HdId);
            if (result.Status == DbStatusCode.Read)
            {
                // Copy certain attributes into the fetched User Profile
                result.Payload.Email = profile.Email;
                result.Payload.TermsOfServiceId = profile.TermsOfServiceId;
                result.Payload.UpdatedBy = profile.UpdatedBy;
                result.Payload.Version = profile.Version;
                result.Payload.YearOfBirth = profile.YearOfBirth;
                result.Status = DbStatusCode.Deferred;

                if (commit)
                {
                    try
                    {
                        this.dbContext.SaveChanges();
                        result.Status = DbStatusCode.Updated;
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        result.Status = DbStatusCode.Concurrency;
                        result.Message = e.Message;
                    }
                    catch (DbUpdateException e)
                    {
                        this.logger.LogError("Unable to update UserProfile to DB {Exception}", e.ToString());
                        result.Status = DbStatusCode.Error;
                        result.Message = e.Message;
                    }
                }
            }

            this.logger.LogDebug("Finished updating user profile in DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<UserProfile> UpdateComplete(UserProfile profile, bool commit = true)
        {
            DbResult<UserProfile> result = new()
            {
                Status = DbStatusCode.Error,
                Payload = profile,
            };

            this.logger.LogTrace("Updating user profile in DB...");
            this.dbContext.UserProfile.Update(profile);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DbStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DbStatusCode.Concurrency;
                    result.Message = e.Message;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError("Unable to update UserProfile to DB {Exception}", e.ToString());
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug("Finished updating user profile in DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<UserProfile> GetUserProfile(string hdId)
        {
            this.logger.LogTrace("Getting user profile from DB... {HdId}", hdId);
            DbResult<UserProfile> result = new();
            UserProfile? profile = this.dbContext.UserProfile.Find(hdId);
            if (profile != null)
            {
                result.Payload = profile;
                result.Status = DbStatusCode.Read;
            }
            else
            {
                this.logger.LogInformation("Unable to find User by HDID {HdId}", hdId);
                result.Status = DbStatusCode.NotFound;
            }

            this.logger.LogDebug("Finished getting user profile from DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<List<UserProfile>> GetUserProfiles(IList<string> hdIds)
        {
            this.logger.LogTrace("Getting user profiles from DB...");
            DbResult<List<UserProfile>> result = new();
            result.Payload = this.dbContext.UserProfile
                .Where(p => hdIds.Contains(p.HdId))
                .ToList();

            result.Status = DbStatusCode.Read;
            this.logger.LogDebug("Finished getting user profiles from DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<List<UserProfile>> GetUserProfiles(UserQueryType queryType, string queryString)
        {
            this.logger.LogTrace("Getting user profiles via message verification from DB for type {QueryType}: {QueryString}", queryType, queryString);
            DbResult<List<UserProfile>> result = new();

            switch (queryType)
            {
                case UserQueryType.Email:
                    result.Payload = this.dbContext.UserProfile
                        .Where(user => user.Verifications.Any(v => v.Email != null && EF.Functions.ILike(v.Email.To, $"%{queryString}%")))
                        .GroupBy(user => user.HdId)
                        .Select(x => x.First())
                        .ToList();
                    break;
                case UserQueryType.Sms:
                    result.Payload = this.dbContext.UserProfile
                        .Where(user => user.Verifications.Any(v => EF.Functions.ILike(v.SmsNumber, $"%{queryString}%")))
                        .GroupBy(user => user.HdId)
                        .Select(x => x.First())
                        .ToList();
                    break;
            }

            result.Status = DbStatusCode.Read;
            this.logger.LogDebug("Finished getting user profiles from DB");
            return result;
        }

        /// <inheritdoc/>
        public DbResult<List<UserProfile>> GetAllUserProfilesAfter(DateTime filterDateTime, int page = 0, int pageSize = 500)
        {
            DbResult<List<UserProfile>> result = new();
            int offset = page * pageSize;
            result.Payload = this.dbContext.UserProfile
                .Where(p => p.LastLoginDateTime < filterDateTime && p.ClosedDateTime == null && !string.IsNullOrWhiteSpace(p.Email))
                .OrderBy(o => o.CreatedDateTime)
                .Skip(offset)
                .Take(pageSize)
                .ToList();
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public DbResult<List<UserProfile>> GetClosedProfiles(DateTime filterDateTime, int page = 0, int pageSize = 500)
        {
            DbResult<List<UserProfile>> result = new();
            int offset = page * pageSize;
            result.Payload = this.dbContext.UserProfile
                .Where(p => p.ClosedDateTime != null && p.ClosedDateTime < filterDateTime)
                .OrderBy(o => o.ClosedDateTime)
                .Skip(offset)
                .Take(pageSize)
                .ToList();
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyRegisteredUsersCount(TimeSpan offset)
        {
            Dictionary<DateTime, int> dateCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, createdDate = GatewayDbContext.DateTrunc("days", x.CreatedDateTime.AddMinutes(offset.TotalMinutes)) })
                .GroupBy(x => x.createdDate)
                .Select(x => new { createdDate = x.Key, count = x.Count() })
                .OrderBy(x => x.createdDate)
                .ToDictionary(x => x.createdDate, x => x.count);

            return dateCount;
        }

        /// <inheritdoc/>
        public IDictionary<DateTime, int> GetDailyLoggedInUsersCount(TimeSpan offset)
        {
            Dictionary<DateTime, int> dateCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime })
                .Concat(
                    this.dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime }))
                .Select(x => new { x.HdId, lastLoginDate = GatewayDbContext.DateTrunc("days", x.LastLoginDateTime.AddMinutes(offset.TotalMinutes)) })
                .Distinct()
                .GroupBy(x => x.lastLoginDate)
                .Select(x => new { lastLoginDate = x.Key, count = x.Count() })
                .OrderBy(x => x.lastLoginDate)
                .ToDictionary(x => x.lastLoginDate, x => x.count);

            return dateCount;
        }

        /// <inheritdoc/>
        public DbResult<IEnumerable<UserProfile>> GetAll(int page, int pageSize)
        {
            this.logger.LogTrace("Retrieving all the user profiles for the page #{Page} with pageSize: {PageSize}...", page, pageSize);
            return DbDelegateHelper.GetPagedDbResult(
                this.dbContext.UserProfile
                    .OrderBy(userProfile => userProfile.CreatedDateTime),
                page,
                pageSize);
        }

        /// <inheritdoc/>
        public int GetRecurrentUserCount(int dayCount, DateTime startDate, DateTime endDate)
        {
            this.logger.LogTrace("Retrieving recurring user count for {DayCount} days between {StartDate} and {EndDate}...", dayCount, startDate, endDate);

            int recurrentCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime })
                .Concat(
                    this.dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime }))
                .Where(x => x.LastLoginDateTime >= startDate && x.LastLoginDateTime <= endDate)
                .Select(x => new { x.HdId, lastLoginDate = GatewayDbContext.DateTrunc("days", x.LastLoginDateTime) })
                .Distinct()
                .GroupBy(x => x.HdId)
                .Select(x => new { HdId = x.Key, count = x.Count() })
                .Count(x => x.count >= dayCount);

            return recurrentCount;
        }

        /// <inheritdoc/>
        public DbResult<IEnumerable<UserProfileHistory>> GetUserProfileHistories(string hdid, int limit)
        {
            DbResult<IEnumerable<UserProfileHistory>> result = new();
            result.Payload = this.dbContext.UserProfileHistory
                .Where(p => p.HdId == hdid)
                .OrderByDescending(p => p.LastLoginDateTime)
                .Take(limit);
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public IDictionary<string, int> GetLoggedInUserYearOfBirthCounts(DateTime startDate, DateTime endDate)
        {
            Dictionary<string, int> yobCount = this.dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime, x.YearOfBirth })
                .Concat(
                    this.dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime, x.YearOfBirth }))
                .Where(x => x.YearOfBirth != null && x.LastLoginDateTime >= startDate && x.LastLoginDateTime <= endDate)
                .Select(x => new { x.HdId, x.YearOfBirth })
                .Distinct()
                .GroupBy(x => x.YearOfBirth)
                .Select(x => new { yearOfBirth = x.Key, count = x.Count() })
                .ToDictionary(x => x.yearOfBirth!, x => x.count);

            return new SortedDictionary<string, int>(yobCount);
        }
    }
}
