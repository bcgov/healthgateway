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
    using HealthGateway.Database.Migrations;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBMessagingVerificationDelegate : IMessagingVerificationDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBMessagingVerificationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBMessagingVerificationDelegate(
            ILogger<DBMessagingVerificationDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public Guid Insert(MessagingVerification messageVerification)
        {
            this.logger.LogTrace($"Inserting message verification to DB... {JsonSerializer.Serialize(messageVerification)}");
            if (messageVerification.VerificationType == MessagingVerificationType.Email &&
                (messageVerification.Email == null || messageVerification.EmailId == null))
            {
                throw new ArgumentException("Email/EmailId cannot be null when verification type is Email");
            }
            else if (messageVerification.VerificationType == MessagingVerificationType.SMS &&
                (string.IsNullOrWhiteSpace(messageVerification.SMSNumber) || string.IsNullOrWhiteSpace(messageVerification.SMSValidationCode)))
            {
                throw new ArgumentException("SMSNumber/SMSValidationCode cannot be null or empty when verification type is SMS");
            }

            this.dbContext.Add<MessagingVerification>(messageVerification);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished inserting message verification to DB. {messageVerification.Id}");
            return messageVerification.Id;
        }

        /// <inheritdoc />
        public MessagingVerification? GetByInviteKey(Guid inviteKey)
        {
            this.logger.LogTrace($"Getting email invite from DB... {inviteKey}");
            MessagingVerification? retVal = this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.InviteKey == inviteKey && p.VerificationType == MessagingVerificationType.Email)
                .FirstOrDefault();

            this.logger.LogDebug($"Finished getting email invite from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public IEnumerable<MessagingVerification> GetAllEmail()
        {
            this.logger.LogTrace($"Getting all email invites from DB...");
            IEnumerable<MessagingVerification> retVal = this.dbContext
                .MessagingVerification
                .Where(p => p.VerificationType == MessagingVerificationType.Email)
                .ToList();

            this.logger.LogDebug($"Finished getting all email invites from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public MessagingVerification? GetLastForUser(string hdid, string messagingVerificationType)
        {
            this.logger.LogTrace($"Getting last messaging verification from DB for user... {hdid}");
            MessagingVerification? retVal = this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.HdId == hdid && p.VerificationType == messagingVerificationType)
                .OrderByDescending(p => p.UpdatedDateTime)
                .FirstOrDefault();

            if (retVal != null && retVal.Deleted)
            {
                return null;
            }

            this.logger.LogDebug($"Finished getting messaging verification from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public void Update(MessagingVerification messageVerification)
        {
            this.logger.LogTrace($"Updating email invite in DB... {JsonSerializer.Serialize(messageVerification)}");
            this.dbContext.Update<MessagingVerification>(messageVerification);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished updating email invite in DB. {messageVerification.Id}");
        }
    }
}
