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
        private const string CommitSizeKey = "CommitSize";
        private const string JobKey = "OneTime";

        private readonly int commitSize;
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
            this.commitSize = configuration.GetValue($"{JobKey}:{CommitSizeKey}", 5000);
        }

        /// <inheritdoc/>
        public void Run()
        {
            this.logger.LogInformation("Performing Task {Name} started.", this.GetType().Name);

            List<Email> emails = this.dbContext.Email.Where(
                    email => this.dbContext.MessagingVerification.Any(msgVerification => msgVerification.EmailId == email.Id && msgVerification.EmailAddress == null))
                .ToList();

            this.logger.LogInformation("The number of emails to copy from Email.To to MessagingVerification.EmailAddress: {Emails}.", emails.Count);

            int processedCount = 0;

            emails.ForEach(
                email =>
                {
                    List<MessagingVerification> verifications = this.dbContext.MessagingVerification.Where(mv => mv.EmailId == email.Id).ToList();
                    verifications.ForEach(v => v.EmailAddress = email.To);
                    processedCount++;
                    this.IncrementalSave(processedCount);
                });

            this.FinalSave(processedCount);

            this.logger.LogInformation("Performing Task {Name} finished.", this.GetType().Name);
        }

        private void FinalSave(int processedCount)
        {
            if (processedCount % this.commitSize != 0)
            {
                this.dbContext.SaveChanges();
                this.logger.LogInformation("Saving message verification changes after {Count} email(s) processed.", processedCount);
            }
        }

        private void IncrementalSave(int processedCount)
        {
            if (processedCount % this.commitSize == 0)
            {
                this.dbContext.SaveChanges();
                this.logger.LogInformation("Saving message verification changes after {Count} email(s) processed.", processedCount);
            }
        }
    }
}
