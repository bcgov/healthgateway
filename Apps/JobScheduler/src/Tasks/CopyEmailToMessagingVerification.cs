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
namespace HealthGateway.JobScheduler.Tasks
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Context;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Copies Health Gateway DB's Email.To to MessageVerification.Email.
    /// </summary>
    public class CopyEmailToMessagingVerification : IOneTimeTask
    {
        private const string BatchSizeKey = "BatchSize";
        private const string JobKey = "OneTime";

        private readonly int batchSize;
        private readonly GatewayDbContext dbContext;
        private readonly ILogger<CopyEmailToMessagingVerification> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyEmailToMessagingVerification"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The configuration to use.</param>
        public CopyEmailToMessagingVerification(
            GatewayDbContext dbContext,
            ILogger<CopyEmailToMessagingVerification> logger,
            IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.batchSize = configuration.GetValue($"{JobKey}:{BatchSizeKey}", 5000);
        }

        /// <inheritdoc/>
        public void Run()
        {
            this.logger.LogInformation("Performing Task {Name} started.", this.GetType().Name);

            IQueryable<Email> query = this.dbContext.Email.Where(
                email => this.dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id && msgVerification.EmailAddress == null));

            List<Email> emails = query.Take(this.batchSize).ToList();
            this.logger.LogInformation("The number of emails to copy from Email.To to MessagingVerification.EmailAddress: {Emails}", emails.Count);
            int processed = 0;

            while (emails.Count > 0)
            {
                emails.ForEach(
                    email =>
                    {
                        List<MessagingVerification> verifications = this.dbContext.MessagingVerification.Where(mv => mv.EmailId == email.Id).ToList();
                        verifications.ForEach(v => v.EmailAddress = email.To);
                        processed++;
                    });

                this.dbContext.SaveChanges();
                this.logger.LogInformation("Saved message verification changes after {Processed} email(s) processed", processed);

                emails = query.Take(this.batchSize).ToList();
                this.logger.LogInformation("The number of emails to copy from Email.To to MessagingVerification.EmailAddress: {Emails}", emails.Count);
            }

            this.logger.LogInformation("Performing Task {Name} finished", this.GetType().Name);
        }
    }
}
