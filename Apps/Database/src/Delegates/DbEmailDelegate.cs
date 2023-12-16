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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        public Email? GetEmail(Guid emailId)
        {
            this.logger.LogTrace("Getting email from DB... {EmailId}", emailId);
            Email? retVal = this.dbContext.Find<Email>(emailId);
            this.logger.LogDebug("Finished getting email {EmailId} from DB", emailId);
            return retVal;
        }

        /// <inheritdoc/>
        public Email? GetStandardEmail(Guid emailId)
        {
            this.logger.LogTrace("Getting new email from DB... {EmailId}", emailId);
            Email? retVal = this.dbContext.Email.SingleOrDefault(p => p.Id == emailId && p.Priority >= EmailPriority.Standard);
            this.logger.LogDebug("Finished getting new email {EmailId} from DB", emailId);
            return retVal;
        }

        /// <inheritdoc/>
        public IList<Email> GetUnsentEmails(int maxRows)
        {
            this.logger.LogTrace("Getting list of low priority emails from DB... {MaxRows}", maxRows);
            IList<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority < EmailPriority.Standard)
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.CreatedDateTime)
                .Take(maxRows)
                .ToList();
            this.logger.LogDebug("Finished getting list of low priority emails from DB");
            return retVal;
        }

        /// <inheritdoc/>
        public Guid InsertEmail(Email email, bool shouldCommit = true)
        {
            this.logger.LogTrace("Inserting email to DB..");
            this.dbContext.Add(email);
            if (shouldCommit)
            {
                this.dbContext.SaveChanges();
            }

            this.logger.LogDebug("Finished inserting email to DB. {Email}", email.Id);
            return email.Id;
        }

        /// <inheritdoc/>
        public async Task<Guid> InsertEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default)
        {
            this.logger.LogTrace("Inserting email to DB..");
            await this.dbContext.AddAsync(email, ct);
            if (shouldCommit)
            {
                await this.dbContext.SaveChangesAsync(ct);
            }

            this.logger.LogDebug("Finished inserting email to DB. {Email}", email.Id);
            return email.Id;
        }

        /// <inheritdoc/>
        public void UpdateEmail(Email email)
        {
            this.logger.LogTrace("Updating email {Email} in DB... ", email.Id);
            this.dbContext.Update(email);
            this.dbContext.SaveChanges();
            this.logger.LogDebug("Finished updating email {Email} in DB", email.Id);
        }

        /// <inheritdoc/>
        public EmailTemplate GetEmailTemplate(string templateName)
        {
            this.logger.LogTrace("Getting email template {TemplateName} from DB... ", templateName);
            EmailTemplate retVal = this.dbContext
                .EmailTemplate
                .First(p => p.Name == templateName);
            this.logger.LogDebug("Finished getting email {TemplateName} template from DB", templateName);

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<EmailTemplate?> GetEmailTemplateAsync(string templateName, CancellationToken ct)
        {
            this.logger.LogTrace("Getting email template {TemplateName} from DB... ", templateName);
            return await this.dbContext
                .EmailTemplate
                .FirstOrDefaultAsync(p => p.Name == templateName, ct);
        }

        /// <inheritdoc/>
        public DbResult<IList<Email>> GetEmails(int offset = 0, int pageSize = 1000)
        {
            this.logger.LogTrace("Getting Emails...");
            DbResult<IList<Email>> result = new();
            result.Payload = this.dbContext.Email
                .OrderByDescending(o => o.CreatedBy)
                .Skip(offset)
                .Take(pageSize)
                .ToList();
            result.Status = DbStatusCode.Read;
            return result;
        }

        /// <inheritdoc/>
        public int Delete(uint daysAgo, int maxRows, bool shouldCommit = true)
        {
            int deletedCount = this.dbContext.Email
                .Where(
                    email => email.EmailStatusCode == EmailStatus.Processed &&
                             email.CreatedDateTime <= DateTime.UtcNow.AddDays(daysAgo * -1).Date)
                .Where(email => this.dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id && msgVerification.EmailAddress == email.To))
                .OrderBy(email => email.CreatedDateTime)
                .Take(maxRows)
                .ExecuteDelete();
            return deletedCount;
        }
    }
}
