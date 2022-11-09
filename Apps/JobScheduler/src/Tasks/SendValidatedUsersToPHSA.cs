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
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Queries the Health Gateway DB for all current users with emails.
    /// Queues NotificationSettings job for each.
    /// </summary>
    public class SendValidatedUsersToPHSA : IOneTimeTask
    {
        private readonly GatewayDbContext dbContext;
        private readonly ILogger<SendValidatedUsersToPHSA> logger;
        private readonly INotificationSettingsService notificationSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendValidatedUsersToPHSA"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="notificationSettingsService">The notification settings service.</param>
        public SendValidatedUsersToPHSA(
            ILogger<SendValidatedUsersToPHSA> logger,
            GatewayDbContext dbContext,
            INotificationSettingsService notificationSettingsService)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.notificationSettingsService = notificationSettingsService;
        }

        /// <summary>
        /// Runs the task that needs to be done for the IOneTimeTask.
        /// </summary>
        public void Run()
        {
            this.logger.LogInformation("Performing Task {Name}", this.GetType().Name);
            IEnumerable<UserProfile> users = this.dbContext.UserProfile.Where(q => !string.IsNullOrEmpty(q.Email)).ToList();
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
                this.notificationSettingsService.QueueNotificationSettings(nsr);
            }

            this.logger.LogInformation("Task {Name} has completed", this.GetType().Name);
        }
    }
}
