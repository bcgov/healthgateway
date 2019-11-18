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
    using System.Diagnostics.Contracts;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;

    /// <inheritdoc />
    public class DBProfileDelegate : IProfileDelegate
    {
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBProfileDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBProfileDelegate(GatewayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> CreateUserProfile(UserProfile profile)
        {
            DBResult<UserProfile> result = new DBResult<UserProfile>();
            this.dbContext.Add<UserProfile>(profile);
            try
            {
                this.dbContext.SaveChanges();
                result.Status = DBStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                result.Status = DBStatusCode.Error;
                result.Message = e.Message;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> UpdateUserProfile(UserProfile profile)
        {
            Contract.Requires(profile != null);
            DBResult<UserProfile> result = this.GetUserProfile(profile.HdId);
            if (result.Status == DBStatusCode.Read)
            {
                // Copy certain attributes into the fetched User Profile
                result.Payload.Email = profile.Email;
                result.Payload.AcceptedTermsOfService = profile.AcceptedTermsOfService;
                result.Payload.UpdatedBy = profile.UpdatedBy;
                result.Payload.Version = profile.Version;
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
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> GetUserProfile(string hdId)
        {
            DBResult<UserProfile> result = new DBResult<UserProfile>();
            UserProfile profile = this.dbContext.UserProfile.Find(hdId);
            result.Payload = profile;
            result.Status = profile != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            return result;
        }
    }
}
