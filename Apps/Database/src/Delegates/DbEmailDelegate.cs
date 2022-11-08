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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        public Email GetEmail(Guid emailId)
        {
            this.logger.LogTrace("Getting email from DB... {EmailId}", emailId);
            Email? retVal = this.dbContext.Find<Email>(emailId);
            this.logger.LogDebug("Finished getting email {EmailId} from DB.", emailId);
            return retVal!;
        }

        /// <inheritdoc/>
        public Email? GetNewEmail(Guid emailId)
        {
            this.logger.LogTrace("Getting new email from DB... {EmailId}", emailId);
            Email? retVal = this.dbContext.Email.Where(p => p.Id == emailId && p.EmailStatusCode == EmailStatus.New && p.Priority >= EmailPriority.Standard).SingleOrDefault();
            this.logger.LogDebug("Finished getting new email {EmailId} from DB.", emailId);
            return retVal;
        }

        /// <inheritdoc/>
        public IList<Email> GetLowPriorityEmail(int maxRows)
        {
            this.logger.LogTrace("Getting list of low priority emails from DB... {MaxRows}", maxRows);
            IList<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority < EmailPriority.Standard)
                .OrderByDescending(o => o.Priority)
                .ThenBy(o => o.CreatedDateTime)
                .Take(maxRows)
                .ToList();
            this.logger.LogDebug("Finished getting list of low priority emails from DB.");
            return retVal;
        }

        /// <inheritdoc/>
        public IList<Email> GetStandardPriorityEmail(int maxRows)
        {
            this.logger.LogTrace("Getting list of standard priority emails from DB... {MaxRows}", maxRows);
            IList<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority >= EmailPriority.Standard && p.Priority < EmailPriority.High)
                .OrderByDescending(s => s.Priority)
                .Take(maxRows)
                .ToList();
            this.logger.LogDebug("Finished getting list of standard priority emails from DB.");
            return retVal;
        }

        /// <inheritdoc/>
        public IList<Email> GetHighPriorityEmail(int maxRows)
        {
            this.logger.LogTrace("Getting list of high priority emails from DB... {MaxRows}", maxRows);
            IList<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority >= EmailPriority.High && p.Priority < EmailPriority.Urgent)
                .OrderByDescending(s => s.Priority)
                .Take(maxRows)
                .ToList();
            this.logger.LogDebug("Finished getting list of high priority emails from DB.");
            return retVal;
        }

        /// <inheritdoc/>
        public IList<Email> GetUrgentPriorityEmail(int maxRows)
        {
            this.logger.LogTrace("Getting list of urgent priority emails from DB... {MaxRows}", maxRows);
            IList<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New && p.Priority >= EmailPriority.Urgent)
                .OrderByDescending(s => s.Priority)
                .Take(maxRows)
                .ToList();
            this.logger.LogDebug("Finished getting list of urgent priority emails from DB.");
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
                .Where(p => p.Name == templateName)
                .First();
            this.logger.LogDebug("Finished getting email {TemplateName} template from DB", templateName);

            return retVal;
        }

        /// <inheritdoc/>
        public DbResult<IList<Email>> GetEmails(int offset = 0, int pagesize = 1000)
        {
            this.logger.LogTrace("Getting Emails...");
            DbResult<IList<Email>> result = new();
            result.Payload = this.dbContext.Email
                .OrderByDescending(o => o.CreatedBy)
                .Skip(offset)
                .Take(pagesize)
                .ToList();
            result.Status = result.Payload != null ? DbStatusCode.Read : DbStatusCode.NotFound;
            return result;
        }

        /// <inheritdoc/>
        public int Delete(uint daysAgo, int maxRows, bool shouldCommit = true)
        {
            IList<Email> oldIds = this.dbContext.Email
                .Where(
                    email => email.EmailStatusCode == EmailStatus.Processed &&
                             email.CreatedDateTime <= GatewayDbContext.DateTrunc("days", DateTime.UtcNow.AddDays(daysAgo * -1)))
                .Where(email => !this.dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id))
                .OrderBy(email => email.CreatedDateTime)
                .Select(email => new Email { Id = email.Id, Version = email.Version })
                .Take(maxRows)
                .ToList();
            if (oldIds.Count > 0)
            {
                this.logger.LogDebug("Deleting {Count} Emails out of a maximum of {MaxRows}", oldIds.Count, maxRows);
                this.dbContext.RemoveRange(oldIds);
                if (shouldCommit)
                {
                    this.dbContext.SaveChanges();
                }
            }
            else
            {
                this.logger.LogDebug("No emails to delete that are older than {DaysAgo} days", daysAgo);
            }

            return oldIds.Count;
        }
    }
}
