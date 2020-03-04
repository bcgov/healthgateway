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
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBEmailInviteDelegate : IEmailInviteDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEmailInviteDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBEmailInviteDelegate(
            ILogger<DBEmailInviteDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public Guid Insert(EmailInvite invite)
        {
            this.logger.LogTrace($"Inserting email invite to DB... {JsonSerializer.Serialize(invite)}");
            this.dbContext.Add<EmailInvite>(invite);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished inserting email invite to DB. {invite.Id}");
            return invite.Id;
        }

        /// <inheritdoc />
        public EmailInvite GetByInviteKey(Guid inviteKey)
        {
            this.logger.LogTrace($"Getting email invite from DB... {inviteKey}");
            EmailInvite retVal = this.dbContext
                .EmailInvite
                .Include(email => email.Email)
                .Where(p => p.InviteKey == inviteKey)
                .FirstOrDefault();

            this.logger.LogDebug($"Finished getting email invite from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public EmailInvite GetLastForUser(string hdid)
        {
            this.logger.LogTrace($"Getting last email invite from DB for user... {hdid}");
            EmailInvite retVal = this.dbContext
                .EmailInvite
                .Include(email => email.Email)
                .Where(p => p.HdId == hdid)
                .OrderByDescending(p => p.UpdatedDateTime)
                .FirstOrDefault();

            this.logger.LogDebug($"Finished getting email invite from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public void Update(EmailInvite emailInvite)
        {
            this.logger.LogTrace($"Updating email invite in DB... {JsonSerializer.Serialize(emailInvite)}");
            this.dbContext.Update<EmailInvite>(emailInvite);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished updating email invite in DB. {emailInvite.Id}");
        }
    }
}
