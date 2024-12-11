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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
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
    public class DbProfileDelegate(ILogger<DbProfileDelegate> logger, GatewayDbContext dbContext) : IUserProfileDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<UserProfile>> InsertUserProfileAsync(UserProfile profile, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding user profile to DB");
            dbContext.Add(profile);

            DbResult<UserProfile> result = new();

            try
            {
                if (commit)
                {
                    await dbContext.SaveChangesAsync(ct);
                }

                result.Payload = profile;
                result.Status = DbStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Error adding user profile to DB");
                result.Status = DbStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<DbResult<UserProfile>> UpdateAsync(UserProfile profile, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating user profile in DB");

            UserProfile? userProfile = await this.GetUserProfileAsync(profile.HdId, true, ct);
            DbResult<UserProfile> result = new();

            if (userProfile != null)
            {
                // Copy certain attributes into the fetched User Profile
                userProfile.Email = profile.Email;
                userProfile.TermsOfServiceId = profile.TermsOfServiceId;
                userProfile.UpdatedBy = profile.UpdatedBy;
                userProfile.Version = profile.Version;
                userProfile.YearOfBirth = profile.YearOfBirth;
                userProfile.LastLoginClientCode = profile.LastLoginClientCode;
                userProfile.BetaFeatureCodes = profile.BetaFeatureCodes;
                result.Status = DbStatusCode.Deferred;
                result.Payload = userProfile;
                dbContext.UserProfile.Update(userProfile);

                if (commit)
                {
                    try
                    {
                        await dbContext.SaveChangesAsync(ct);
                        result.Status = DbStatusCode.Updated;
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        logger.LogWarning(e, "Error updating user profile in DB");
                        result.Status = DbStatusCode.Concurrency;
                        result.Message = e.Message;
                    }
                    catch (DbUpdateException e)
                    {
                        logger.LogError(e, "Error updating user profile in DB");
                        result.Status = DbStatusCode.Error;
                        result.Message = e.Message;
                    }
                }
            }
            else
            {
                logger.LogDebug("User profile does not exist in DB for {Hdid}", profile.HdId);
                result.Status = DbStatusCode.NotFound;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileAsync(string hdid, bool includeBetaFeatureCodes = false, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profile from DB for {Hdid}", hdid);

            IQueryable<UserProfile> query = dbContext.UserProfile;

            if (includeBetaFeatureCodes)
            {
                query = query.Include(p => p.BetaFeatureCodes);
            }

            return await query.SingleOrDefaultAsync(p => p.HdId == hdid, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserProfile>> GetUserProfilesAsync(string email, bool includeBetaFeatureCodes = false, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profiles from DB by email address");

            IQueryable<UserProfile> query = dbContext.UserProfile;
            query = query.Where(p => EF.Functions.ILike(p.Email, email));

            if (includeBetaFeatureCodes)
            {
                query = query.Include(p => p.BetaFeatureCodes);
            }

            return await query.ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserProfile>> GetUserProfilesAsync(IList<string> hdIds, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profiles from DB for {Hdids}", hdIds);
            return await dbContext.UserProfile
                .Where(p => hdIds.Contains(p.HdId))
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserProfile>> GetUserProfilesAsync(UserQueryType queryType, string queryString, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profiles from DB by {QueryType}", queryType);

            IQueryable<UserProfile> dbQuery = dbContext.UserProfile;
            dbQuery = queryType switch
            {
                UserQueryType.Email => dbQuery.Where(user => user.Verifications.Any(v => EF.Functions.ILike(v.EmailAddress, $"%{queryString}%"))),
                UserQueryType.Sms => dbQuery.Where(user => user.Verifications.Any(v => EF.Functions.ILike(v.SmsNumber, $"%{queryString}%"))),
                _ => throw new ArgumentOutOfRangeException(nameof(queryType)),
            };
            dbQuery = dbQuery.GroupBy(user => user.HdId).Select(x => x.First());

            return await dbQuery.ToArrayAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<List<UserProfile>> GetClosedProfilesAsync(DateTime filterDateTime, int page = 0, int pageSize = 500, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profiles from DB closed before {CloseProfileCutoffDate}, page #{PageNumber} with page size {PageSize}", filterDateTime, page, pageSize);

            int offset = page * pageSize;
            return await dbContext.UserProfile
                .Where(p => p.ClosedDateTime != null && p.ClosedDateTime < filterDateTime)
                .OrderBy(o => o.ClosedDateTime)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<DateOnly, int>> GetDailyUserRegistrationCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving daily user registration counts from DB between {StartDate} and {EndDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            return await dbContext.UserProfile
                .Where(u => u.CreatedDateTime >= startDateTimeOffset.UtcDateTime && u.CreatedDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.HdId, createdDate = x.CreatedDateTime.AddMinutes(startDateTimeOffset.TotalOffsetMinutes).Date })
                .GroupBy(x => x.createdDate)
                .Select(x => new { createdDate = x.Key, count = x.Count() })
                .ToDictionaryAsync(x => DateOnly.FromDateTime(x.createdDate), x => x.count, ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<DateOnly, int>> GetDailyUniqueLoginCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving daily unique login counts from DB between {StartDate} and {EndDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            TimeSpan offset = startDateTimeOffset.Offset;
            var userProfileQuery = dbContext.UserProfile
                .Where(u => u.LastLoginDateTime >= startDateTimeOffset.UtcDateTime && u.LastLoginDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(
                    u => new
                    {
                        u.HdId,
                        u.LastLoginDateTime,
                    });

            var userProfileHistoryQuery = dbContext.UserProfileHistory
                .Where(u => u.LastLoginDateTime >= startDateTimeOffset.UtcDateTime && u.LastLoginDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(
                    u => new
                    {
                        u.HdId,
                        u.LastLoginDateTime,
                    });

            var unionQuery = userProfileQuery
                .Union(userProfileHistoryQuery);

            return await unionQuery
                .GroupBy(
                    t => new
                    {
                        LastLoginDate = t.LastLoginDateTime.AddMinutes(offset.TotalMinutes).Date,
                    })
                .Select(
                    g => new
                    {
                        g.Key.LastLoginDate,
                        Count = g.Select(t => t.HdId).Distinct().Count(),
                    })
                .ToDictionaryAsync(x => DateOnly.FromDateTime(x.LastLoginDate), x => x.Count, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving all user profiles from DB, page #{PageNumber} with page size {PageSize}", page, pageSize);
            return await DbDelegateHelper.GetPagedDbResultAsync(dbContext.UserProfile.OrderBy(userProfile => userProfile.CreatedDateTime), page, pageSize, ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetRecurringUserCountAsync(int dayCount, DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving recurring user counts from DB between {StartDate} and {EndDate}, where users have logged in on at least {RecurringUserDays} different days",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime),
                dayCount);

            return await dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime })
                .Concat(
                    dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime }))
                .Where(x => x.LastLoginDateTime >= startDateTimeOffset.UtcDateTime && x.LastLoginDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.HdId, lastLoginDate = x.LastLoginDateTime.Date })
                .Distinct()
                .GroupBy(x => x.HdId)
                .Select(x => new { HdId = x.Key, count = x.Count() })
                .CountAsync(x => x.count >= dayCount, ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<UserLoginClientType, int>> GetLoginClientCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving login client counts from DB between {StartDate} and {EndDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            return await dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginClientCode, x.LastLoginDateTime })
                .Concat(
                    dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginClientCode, x.LastLoginDateTime }))
                .Where(
                    x =>
                        x.LastLoginClientCode != null &&
                        x.LastLoginDateTime >= startDateTimeOffset.UtcDateTime &&
                        x.LastLoginDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.HdId, x.LastLoginClientCode })
                .Distinct()
                .GroupBy(x => x.LastLoginClientCode)
                .Select(x => new { lastLoginClientCode = x.Key, count = x.Count() })
                .ToDictionaryAsync(x => x.lastLoginClientCode!.Value, x => x.count, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<UserProfileHistory>> GetUserProfileHistoryListAsync(string hdid, int limit, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving up to {ProfileHistoryLimit} user profile history entries from DB for {Hdid}", limit, hdid);

            return await dbContext.UserProfileHistory
                .Where(p => p.HdId == hdid)
                .OrderByDescending(p => p.LastLoginDateTime)
                .Take(limit)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetUserProfileCountAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving user profile count from DB");
            return await dbContext.UserProfile.CountAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<int> GetClosedUserProfileCountAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving deleted user profile count from DB");
            return await dbContext.UserProfileHistory
                .Where(h => h.Operation == "DELETE" && !dbContext.UserProfile.Any(p => p.HdId == h.HdId))
                .CountAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<IDictionary<int, int>> GetLoggedInUserYearOfBirthCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default)
        {
            logger.LogDebug(
                "Retrieving year of birth counts from DB between {StartDate} and {EndDate}",
                DateFormatter.ToShortDateAndTime(startDateTimeOffset.UtcDateTime),
                DateFormatter.ToShortDateAndTime(endDateTimeOffset.UtcDateTime));

            Dictionary<int, int> yobCounts = await dbContext.UserProfile
                .Select(x => new { x.HdId, x.LastLoginDateTime, x.YearOfBirth })
                .Concat(
                    dbContext.UserProfileHistory.Select(x => new { x.HdId, x.LastLoginDateTime, x.YearOfBirth }))
                .Where(x => x.YearOfBirth != null && x.LastLoginDateTime >= startDateTimeOffset.UtcDateTime && x.LastLoginDateTime <= endDateTimeOffset.UtcDateTime)
                .Select(x => new { x.HdId, x.YearOfBirth })
                .Distinct()
                .GroupBy(x => x.YearOfBirth)
                .Select(x => new { yearOfBirth = x.Key, count = x.Count() })
                .ToDictionaryAsync(x => x.yearOfBirth!.Value, x => x.count, ct);

            return new SortedDictionary<int, int>(yobCounts);
        }
    }
}
