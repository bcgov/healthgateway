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
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbMessagingVerificationDelegate(ILogger<DbMessagingVerificationDelegate> logger, GatewayDbContext dbContext) : IMessagingVerificationDelegate
    {
        /// <inheritdoc/>
        public async Task<Guid> InsertAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding messaging verification to DB");

            if (messageVerification is { VerificationType: MessagingVerificationType.Email, Email: null })
            {
                throw new ArgumentException("Email cannot be null when verification type is Email");
            }

            if (messageVerification.VerificationType == MessagingVerificationType.Sms &&
                (string.IsNullOrWhiteSpace(messageVerification.SmsNumber) || string.IsNullOrWhiteSpace(messageVerification.SmsValidationCode)))
            {
                throw new ArgumentException("SMSNumber/SMSValidationCode cannot be null or empty when verification type is SMS");
            }

            dbContext.Add(messageVerification);
            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }

            return messageVerification.Id;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification?> GetLastByInviteKeyAsync(Guid inviteKey, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving email messaging verification from DB with invite key {InviteKey}", inviteKey);
            MessagingVerification? retVal = await dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.InviteKey == inviteKey && p.VerificationType == MessagingVerificationType.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .FirstOrDefaultAsync(ct);

            return retVal;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Updating messaging verification in DB with ID {MessagingVerificationId}", messageVerification.Id);
            dbContext.Update(messageVerification);

            if (commit)
            {
                await dbContext.SaveChangesAsync(ct);
            }
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification?> GetLastForUserAsync(string hdid, string messagingVerificationType, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving most recent messaging verification from DB of type {MessagingVerificationType} for {Hdid}", messagingVerificationType, hdid);
            MessagingVerification? verification = await dbContext
                .MessagingVerification
                .Include(email => email.Email)
                .Where(p => p.UserProfileId == hdid && p.VerificationType == messagingVerificationType)
                .OrderByDescending(p => p.UpdatedDateTime)
                .FirstOrDefaultAsync(ct);

            return verification?.Deleted == true ? null : verification;
        }

        /// <inheritdoc/>
        public async Task ExpireAsync(MessagingVerification messageVerification, bool markDeleted, bool commit = true, CancellationToken ct = default)
        {
            messageVerification.ExpireDate = DateTime.UtcNow;
            messageVerification.Deleted = markDeleted;
            await this.UpdateAsync(messageVerification, commit, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<MessagingVerification>> GetUserMessageVerificationsAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving messaging verifications for {Hdid}", hdid);

            return await dbContext.MessagingVerification.Where(mv => mv.UserProfileId == hdid)
                .Include(mv => mv.Email)
                .OrderByDescending(mv => mv.CreatedDateTime)
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
