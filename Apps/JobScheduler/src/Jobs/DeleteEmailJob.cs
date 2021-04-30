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
namespace Healthgateway.JobScheduler.Jobs
{
    using Hangfire;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Deletes emails after a configurable amount of days.
    /// </summary>
    public class DeleteEmailJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly ILogger<EmailJob> logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly int deleteMaxRows;
        private readonly uint deleteAfterDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteEmailJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public DeleteEmailJob(
            IConfiguration configuration,
            ILogger<EmailJob> logger,
            IEmailDelegate emailDelegate)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate!;
            IConfigurationSection section = configuration!.GetSection("Smtp");
            section = configuration.GetSection("DeleteEmailJob");
            this.deleteMaxRows = section.GetValue<int>("DeleteMaxRows", 1000);
            this.deleteAfterDays = section.GetValue<uint>("DeleteAfterDays", 30);
        }

        /// <summary>
        /// Deletes a configurable amount of emails (DeleteMaxRows) after a configurable amount of time in days (DeleteAfterDays).
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void DeleteOldEmails()
        {
            this.logger.LogInformation($"Delete job running: Delete emails {this.deleteAfterDays} days old and limit to {this.deleteMaxRows} deleted");
            int count = this.emailDelegate.Delete(this.deleteAfterDays, this.deleteMaxRows, true);
            this.logger.LogInformation($"Delete job finished after removing {count} records");
        }
    }
}
