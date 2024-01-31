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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Queries the Health Gateway DB for all current users with emails.
    /// Queues NotificationSettings job for each.
    /// </summary>
    public class SendValidatedUsersToPhsa : IOneTimeTask
    {
        private readonly GatewayDbContext dbContext;
        private readonly ILogger<SendValidatedUsersToPhsa> logger;
        private readonly INotificationSettingsService notificationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendValidatedUsersToPhsa"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="notificationSettingsService">The notification settings service.</param>
        public SendValidatedUsersToPhsa(
            ILogger<SendValidatedUsersToPhsa> logger,
            GatewayDbContext dbContext,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.notificationSettingsService = notificationSettingsService;
        }

        /// <inheritdoc/>
        public async Task RunAsync(CancellationToken ct = default)
        {
            this.logger.LogInformation("Performing Task {Name}", this.GetType().Name);
            IEnumerable<UserProfile> users = await this.dbContext.UserProfile.Where(q => !string.IsNullOrEmpty(q.Email)).ToListAsync(ct);
            this.logger.LogInformation("Queueing NotificationSettings for {Count} users", users.Count());
            foreach (UserProfile user in users)
            {
                NotificationSettingsRequest nsr = new()
                {
                    SubjectHdid = user.HdId,
                    EmailAddress = user.Email,
                    EmailEnabled = true,
                };

                // Queue each push to PHSA in the Job Scheduler
                await this.notificationSettingsService.QueueNotificationSettingsAsync(nsr, ct);
            }

            this.logger.LogInformation("Task {Name} has completed", this.GetType().Name);
        }
    }
}
