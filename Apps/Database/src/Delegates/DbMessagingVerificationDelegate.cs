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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbMessagingVerificationDelegate : IMessagingVerificationDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbMessagingVerificationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbMessagingVerificationDelegate(
            ILogger<DbMessagingVerificationDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Guid> InsertAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Inserting message verification to DB...");
            if (messageVerification.VerificationType == MessagingVerificationType.Email && messageVerification.Email == null)
            {
                throw new ArgumentException("Email cannot be null when verification type is Email");
            }

            if (messageVerification.VerificationType == MessagingVerificationType.Sms &&
                (string.IsNullOrWhiteSpace(messageVerification.SmsNumber) || string.IsNullOrWhiteSpace(messageVerification.SmsValidationCode)))
            {
                throw new ArgumentException("SMSNumber/SMSValidationCode cannot be null or empty when verification type is SMS");
            }

            await this.dbContext.AddAsync(messageVerification, ct);
            if (commit)
            {
                await this.dbContext.SaveChangesAsync(ct);
            }

            this.logger.LogDebug("Finished inserting message verification to DB");
            return messageVerification.Id;
        }

        /// <inheritdoc/>
        public MessagingVerification? GetLastByInviteKey(Guid inviteKey)
        {
            this.logger.LogTrace("Getting email message verification from DB... {InviteKey}", inviteKey);
            MessagingVerification? retVal = this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.InviteKey == inviteKey && p.VerificationType == MessagingVerificationType.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .FirstOrDefault();

            this.logger.LogDebug("Finished getting email message verification from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public IEnumerable<MessagingVerification> GetAllEmail()
        {
            this.logger.LogTrace("Getting all email message verifications from DB...");
            IEnumerable<MessagingVerification> retVal = this.dbContext
                .MessagingVerification
                .Where(p => p.VerificationType == MessagingVerificationType.Email)
                .ToList();

            this.logger.LogDebug("Finished getting all email message verifications from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating email message verification in DB...");
            this.dbContext.Update(messageVerification);
            if (commit)
            {
                await this.dbContext.SaveChangesAsync(ct);
            }

            this.logger.LogDebug("Finished updating email message verification {Id} in DB", messageVerification.Id);
        }

        /// <inheritdoc/>
        public MessagingVerification? GetLastForUser(string hdid, string messagingVerificationType)
        {
            this.logger.LogTrace("Getting last messaging verification from DB for user... {HdId}", hdid);
            MessagingVerification? retVal = this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.UserProfileId == hdid && p.VerificationType == messagingVerificationType)
                .OrderByDescending(p => p.UpdatedDateTime)
                .FirstOrDefault();

            if (retVal != null && retVal.Deleted)
            {
                return null;
            }

            this.logger.LogDebug("Finished getting messaging verification from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task ExpireAsync(MessagingVerification messageVerification, bool markDeleted, CancellationToken ct = default)
        {
            messageVerification.ExpireDate = DateTime.UtcNow;
            messageVerification.Deleted = markDeleted;
            await this.UpdateAsync(messageVerification, ct: ct);

            this.logger.LogDebug("Finished Expiring messaging verification from DB");
        }

        /// <inheritdoc/>
        public async Task<IList<MessagingVerification>> GetUserMessageVerificationsAsync(string hdid)
        {
            return await this.dbContext.MessagingVerification.Where(mv => mv.UserProfileId == hdid)
                .Include(mv => mv.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(true);
        }
    }
}
