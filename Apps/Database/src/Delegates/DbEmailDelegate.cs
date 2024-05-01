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
    public class DbEmailDelegate : IEmailDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbEmailDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbEmailDelegate(
            ILogger<DbEmailDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<Email?> GetStandardEmailAsync(Guid emailId, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting new email from DB... {EmailId}", emailId);
            return await this.dbContext.Email.SingleOrDefaultAsync(p => p.Id == emailId && p.Priority >= EmailPriority.Standard, ct);
        }

        /// <inheritdoc/>
        public async Task<IList<Email>> GetUnsentEmailsAsync(int maxRows, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting list of low priority emails from DB... {MaxRows}", maxRows);
            return await this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority < EmailPriority.Standard)
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.CreatedDateTime)
                .Take(maxRows)
                .ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<Guid> InsertEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Inserting email to DB..");
            this.dbContext.Add(email);
            if (shouldCommit)
            {
                await this.dbContext.SaveChangesAsync(ct);
            }

            this.logger.LogDebug("Finished inserting email to DB. {Email}", email.Id);
            return email.Id;
        }

        /// <inheritdoc/>
        public async Task UpdateEmailAsync(Email email, CancellationToken ct = default)
        {
            this.logger.LogTrace("Updating email {Email} in DB... ", email.Id);
            this.dbContext.Update(email);
            await this.dbContext.SaveChangesAsync(ct);
            this.logger.LogDebug("Finished updating email {Email} in DB", email.Id);
        }

        /// <inheritdoc/>
        public async Task<EmailTemplate?> GetEmailTemplateAsync(string templateName, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting email template {TemplateName} from DB... ", templateName);
            return await this.dbContext
                .EmailTemplate
                .FirstOrDefaultAsync(p => p.Name == templateName, ct);
        }

        /// <inheritdoc/>
        public async Task<int> DeleteAsync(uint daysAgo, int maxRows, bool shouldCommit = true, CancellationToken ct = default)
        {
            this.logger.LogDebug("Deleting {MaxRows} emails from {DaysAgo} days ago...", maxRows, daysAgo);
            int deletedCount = await this.dbContext.Email
                .Where(
                    email => email.EmailStatusCode == EmailStatus.Processed &&
                             email.CreatedDateTime <= DateTime.UtcNow.AddDays(daysAgo * -1).Date)
                .Where(email => this.dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id && msgVerification.EmailAddress == email.To))
                .OrderBy(email => email.CreatedDateTime)
                .Take(maxRows)
                .ExecuteDeleteAsync(ct);
            return deletedCount;
        }
    }
}
