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
namespace HealthGateway.JobScheduler.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Deletes emails after a configurable amount of days.
    /// </summary>
    public class DeleteEmailJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly uint deleteAfterDays;
        private readonly int deleteMaxRows;
        private readonly IEmailDelegate emailDelegate;
        private readonly ILogger<DeleteEmailJob> logger;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteEmailJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public DeleteEmailJob(
            IConfiguration configuration,
            ILogger<DeleteEmailJob> logger,
            IEmailDelegate emailDelegate)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            IConfigurationSection section = configuration.GetSection("DeleteEmailJob");
            this.configuration = configuration;
            this.deleteMaxRows = section.GetValue("DeleteMaxRows", 1000);
            this.deleteAfterDays = section.GetValue<uint>("DeleteAfterDays", 30);
        }

        /// <summary>
        /// Deletes a configurable amount of emails (DeleteMaxRows) after a configurable amount of time in days (DeleteAfterDays).
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task DeleteOldEmailsAsync(CancellationToken ct = default)
        {
            this.logger.LogInformation("Delete job running: Delete emails {DeleteAfterDays} days old and limit to {DeleteMaxRows} deleted", this.deleteAfterDays, this.deleteMaxRows);
            TimeSpan localTimeOffset = DateFormatter.GetLocalTimeOffset(this.configuration, DateTime.UtcNow);
            int count = await this.emailDelegate.DeleteAsync(this.deleteAfterDays, this.deleteMaxRows, localTimeOffset, ct: ct);
            this.logger.LogInformation("Delete job finished after removing {Count} records", count);
        }
    }
}
