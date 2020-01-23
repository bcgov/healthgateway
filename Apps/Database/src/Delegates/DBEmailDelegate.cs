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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBEmailDelegate : IEmailDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBEmailDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBEmailDelegate(
            ILogger<DBEmailDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public Email GetEmail(Guid emailId)
        {
            this.logger.LogTrace($"Getting email from DB... {emailId}");
            Email retVal = this.dbContext.Find<Email>(emailId);
            this.logger.LogDebug($"Finished getting email from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public Email GetNewEmail(Guid emailId)
        {
            this.logger.LogTrace($"Getting new email from DB... {emailId}");
            Email retVal = this.dbContext.Email.Where(p => p.Id == emailId &&
                                              p.EmailStatusCode == EmailStatus.New &&
                                              p.Priority >= EmailPriority.Standard).SingleOrDefault();
            this.logger.LogDebug($"Finished getting new email from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public List<Email> GetLowPriorityEmail(int maxRows)
        {
            this.logger.LogTrace($"Getting list of low priority emails from DB... {maxRows}");
            List<Email> retVal = this.dbContext.Email.Where(p => p.EmailStatusCode == EmailStatus.New &&
                                                   p.Priority < EmailPriority.Standard)
                                        .OrderByDescending(s => s.Priority)
                                        .Take(maxRows)
                                        .ToList();
            this.logger.LogDebug($"Finished getting list of low priority emails from DB. {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        /// <inheritdoc />
        public Guid InsertEmail(Email email)
        {
            this.logger.LogTrace($"Inserting email to DB... {email}");
            this.dbContext.Add<Email>(email);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished inserting email to DB. {JsonSerializer.Serialize(email)}");
            return email.Id;
        }

        /// <inheritdoc />
        public void UpdateEmail(Email email)
        {
            this.logger.LogTrace($"Updating email in DB... {email}");
            this.dbContext.Update<Email>(email);
            this.dbContext.SaveChanges();
            this.logger.LogDebug($"Finished updating email in DB. {JsonSerializer.Serialize(email)}");
        }

        /// <inheritdoc />
        public EmailTemplate GetEmailTemplate(string templateName)
        {
            this.logger.LogTrace($"Getting email template from DB... {templateName}");
            EmailTemplate retVal = this.dbContext
                .EmailTemplate
                .Where(p => p.Name == templateName)
                .FirstOrDefault<EmailTemplate>();
            this.logger.LogDebug($"Finished getting email template from DB. {JsonSerializer.Serialize(retVal)}");

            return retVal;
        }
    }
}
