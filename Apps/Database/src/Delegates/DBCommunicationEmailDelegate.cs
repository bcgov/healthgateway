//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBCommunicationEmailDelegate : ICommunicationEmailDelegate
    {
        private readonly ILogger<DBNoteDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCommunicationEmailDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBCommunicationEmailDelegate(
            ILogger<DBNoteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<CommunicationEmail> Add(CommunicationEmail communicationEmail, bool commit = true)
        {
            this.logger.LogTrace($"Adding Communication Email to DB...");
            DBResult<CommunicationEmail> result = new DBResult<CommunicationEmail>()
            {
                Payload = communicationEmail,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.CommunicationEmail.Add(communicationEmail);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to save Communication Email to DB {e}");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished adding Communication in DB");
            return result;
        }

        /// <inheritdoc />
        public List<UserProfile> GetProfilesForCommunication(Guid communicationId, int maxRows)
        {
            this.logger.LogTrace($"Getting active user profiles to send emails from DB...");

            var userProfileList = this.dbContext.UserProfile
                .Where(userProfile => !userProfile.ClosedDateTime.HasValue)
                .Where(userProfile => !this.dbContext.CommunicationEmail.Any(communicationEmail => communicationEmail.UserProfileHdId == userProfile.HdId && communicationEmail.CommunicationId == communicationId))
                .Take(maxRows)
                .ToList();

            return userProfileList;
        }
    }
}
