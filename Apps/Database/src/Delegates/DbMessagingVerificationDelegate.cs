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
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
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

            this.dbContext.Add(messageVerification);
            if (commit)
            {
                await this.dbContext.SaveChangesAsync(ct);
            }

            this.logger.LogDebug("Finished inserting message verification to DB");
            return messageVerification.Id;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification?> GetLastByInviteKeyAsync(Guid inviteKey, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting email message verification from DB... {InviteKey}", inviteKey);
            MessagingVerification? retVal = await this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.InviteKey == inviteKey && p.VerificationType == MessagingVerificationType.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .FirstOrDefaultAsync(ct);

            this.logger.LogDebug("Finished getting email message verification from DB");
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
        public async Task<MessagingVerification?> GetLastForUserAsync(string hdid, string messagingVerificationType, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting last messaging verification from DB for user... {HdId}", hdid);
            MessagingVerification? retVal = await this.dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.UserProfileId == hdid && p.VerificationType == messagingVerificationType)
                .OrderByDescending(p => p.UpdatedDateTime)
                .FirstOrDefaultAsync(ct);

            if (retVal is { Deleted: true })
            {
                return null;
            }

            this.logger.LogDebug("Finished getting messaging verification from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task ExpireAsync(MessagingVerification messageVerification, bool markDeleted, bool commit = true, CancellationToken ct = default)
        {
            messageVerification.ExpireDate = DateTime.UtcNow;
            messageVerification.Deleted = markDeleted;
            await this.UpdateAsync(messageVerification, commit, ct);

            this.logger.LogDebug("Finished Expiring messaging verification from DB");
        }

        /// <inheritdoc/>
        public async Task<IList<MessagingVerification>> GetUserMessageVerificationsAsync(string hdid, CancellationToken ct = default)
        {
            return await this.dbContext.MessagingVerification.Where(mv => mv.UserProfileId == hdid)
                .Include(mv => mv.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
