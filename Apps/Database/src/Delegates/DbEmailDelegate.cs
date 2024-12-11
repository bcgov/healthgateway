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
    public class DbEmailDelegate(ILogger<DbEmailDelegate> logger, GatewayDbContext dbContext) : IEmailDelegate
    {
        /// <inheritdoc/>
        public async Task<Email?> GetStandardEmailAsync(Guid emailId, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving email from DB with ID {EmailId}", emailId);
            return await dbContext.Email.SingleOrDefaultAsync(p => p.Id == emailId && p.Priority >= EmailPriority.Standard, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<Email>> GetUnsentEmailsAsync(int maxRows, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving up to {EmailBatchLimit} unsent low-priority emails from DB", maxRows);
            return await dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority < EmailPriority.Standard)
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.CreatedDateTime)
                .Take(maxRows)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<Guid> InsertEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding email to DB");
            dbContext.Add(email);

            if (shouldCommit)
            {
                await dbContext.SaveChangesAsync(ct);
            }

            return email.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAsync(Email email, CancellationToken ct = default)
        {
            logger.LogDebug("Updating email in DB with ID {EmailId}", email.Id);
            dbContext.Update(email);

            await dbContext.SaveChangesAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<EmailTemplate?> GetEmailTemplateAsync(string templateName, CancellationToken ct = default)
        {
            logger.LogDebug("Retrieving email template from DB with template name {EmailTemplateName}", templateName);
            return await dbContext
                .EmailTemplate
                .FirstOrDefaultAsync(p => p.Name == templateName, ct);
        }

        /// <inheritdoc/>
        public async Task<int> DeleteAsync(uint daysAgo, int maxRows, bool shouldCommit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Removing up to {EmailBatchLimit} sent emails from DB created more than {EmailExpiryDays} day(s) ago", maxRows, daysAgo);
            return await dbContext.Email
                .Where(
                    email => email.EmailStatusCode == EmailStatus.Processed &&
                             email.CreatedDateTime <= DateTime.UtcNow.AddDays(daysAgo * -1).Date)
                .Where(email => dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id && msgVerification.EmailAddress == email.To))
                .OrderBy(email => email.CreatedDateTime)
                .Take(maxRows)
                .ExecuteDeleteAsync(ct);
        }
    }
}
