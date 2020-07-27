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
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
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
                result.Payload.Email = profile.Email;
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
            UserProfile profile = this.dbContext.UserProfile.Find(hdId);
            result.Payload = profile;
            result.Status = profile != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            this.logger.LogDebug($"Finished getting user profile from DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<List<UserProfile>> GetAllUserProfilesAfter(DateTime filterDateTime, int page = 0, int pagesize = 500)
        {
            DBResult<List<UserProfile>> result = new DBResult<List<UserProfile>>();
            int offset = page * pagesize;
            result.Payload = this.dbContext.UserProfile
                                .Where(p => (p.LastLoginDateTime == null ||
                                                (p.LastLoginDateTime != null && p.LastLoginDateTime < filterDateTime)) &&
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
        public int GetRegisteredUsersCount()
        {
            int result = this.dbContext.UserProfile
                .Count(w => w.AcceptedTermsOfService);
            return result;
        }

        /// <inheritdoc />
        public int GeUnregisteredInvitedUsersCount()
        {
            int result = this.dbContext.BetaRequest
                .Count(b =>
                    this.dbContext.MessagingVerification.Any(e => e.HdId == b.HdId) &&
                    !this.dbContext.UserProfile.Any(u => u.HdId == b.HdId && u.AcceptedTermsOfService));
            return result;
        }

        /// <inheritdoc />
        public int GetLoggedInUsersCount(TimeSpan offset)
        {
            DateTime now = DateTime.UtcNow;
            DateTime clientTime = DateTime.SpecifyKind(now.Add(offset), DateTimeKind.Unspecified);
            DateTimeOffset clientStartDate = new DateTimeOffset(clientTime.Date, offset);
            DateTime queryStartTime = clientStartDate.UtcDateTime;
            DateTime queryEndTime = now;
            int result = this.dbContext.UserProfile
                .Count(u => u.LastLoginDateTime.HasValue &&
               u.LastLoginDateTime.Value >= queryStartTime &&
               u.LastLoginDateTime.Value < queryEndTime);
            return result;
        }
    }
}
