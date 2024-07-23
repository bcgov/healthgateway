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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Resolves issues relating to registrations that failed after creating the user profile.
    /// </summary>
    /// <param name="dbContext">The injected DB context.</param>
    /// <param name="logger">The injected logger.</param>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="messageSender">The injected message sender.</param>
    /// <param name="notificationSettingsService">The injected notification settings service.</param>
    public class FinalizeIncompleteUserRegistrations(
        GatewayDbContext dbContext,
        ILogger<FinalizeIncompleteUserRegistrations> logger,
        IConfiguration configuration,
        IMessageSender messageSender,
        INotificationSettingsService notificationSettingsService) : IOneTimeTask
    {
        private const string BatchSizeKey = "BatchSize";
        private const string JobKey = "OneTime";
        private static readonly DateTime StartDateTime = new(2024, 6, 27, 21, 49, 6, DateTimeKind.Utc);
        private static readonly DateTime HotfixDateTime = new(2024, 6, 28, 21, 33, 21, DateTimeKind.Utc);
        private static readonly DateTime CurrentDateTime = DateTime.UtcNow;

        private readonly int batchSize = configuration.GetValue($"{JobKey}:{BatchSizeKey}", 5000);

        /// <inheritdoc/>
        public async Task RunAsync(CancellationToken ct = default)
        {
            logger.LogDebug("Performing Task {Name} started", nameof(FinalizeIncompleteUserRegistrations));

            await this.SendAccountCreatedEventsAsync(ct);
            await this.UpdateEmailAddressesAsync(ct);
            await this.QueueNotificationSettingsRequestsAsync(ct);

            logger.LogDebug("Performing Task {Name} finished", nameof(FinalizeIncompleteUserRegistrations));
        }

        private async Task SendAccountCreatedEventsAsync(CancellationToken ct)
        {
            logger.LogInformation("Sending AccountCreatedEvents");
            DateTime extendedStartDateTime = StartDateTime.AddMinutes(-5);
            DateTime extendedHotfixDateTime = HotfixDateTime.AddMinutes(5);

            IList<UserProfile> profiles = await dbContext.UserProfile
                .AsNoTracking()
                .Where(p => p.CreatedDateTime > extendedStartDateTime && p.CreatedDateTime < extendedHotfixDateTime)
                .ToListAsync(ct);

            foreach (UserProfile profile in profiles)
            {
                await this.SendAccountCreatedEventAsync(profile.HdId, profile.CreatedDateTime, ct);
            }

            logger.LogInformation("Sent AccountCreatedEvents for {Count} profiles", profiles.Count);
        }

        private async Task UpdateEmailAddressesAsync(CancellationToken ct)
        {
            DateTime extendedHotfixDateTime = HotfixDateTime.AddMinutes(-5);

            int iteration = 0;
            IList<UserProfile> profiles;
            do
            {
                profiles = await dbContext.UserProfile
                    .Where(p => p.CreatedDateTime > extendedHotfixDateTime && p.CreatedDateTime < CurrentDateTime)
                    .Where(p => string.IsNullOrEmpty(p.Email))
                    .Skip(iteration * this.batchSize)
                    .Take(this.batchSize)
                    .Include(p => p.Verifications)
                    .ToListAsync(ct);

                foreach (UserProfile profile in profiles)
                {
                    MessagingVerification? lastEmailVerification = profile.Verifications
                        .Where(v => v.VerificationType == MessagingVerificationType.Email)
                        .MaxBy(v => v.UpdatedDateTime);

                    if (lastEmailVerification is { Validated: false, Deleted: false } && lastEmailVerification.CreatedDateTime == lastEmailVerification.UpdatedDateTime)
                    {
                        logger.LogInformation("Setting validated email address for {Hdid}", profile.HdId);
                        lastEmailVerification.Validated = true;
                        profile.Email = lastEmailVerification.EmailAddress;
                    }
                }

                logger.LogInformation("Addressed potentially empty email addresses for {Count} profiles", profiles.Count);
                await dbContext.SaveChangesAsync(ct); // commit after every iteration
                iteration++;
            }
            while (profiles.Count == this.batchSize);
        }

        private async Task QueueNotificationSettingsRequestsAsync(CancellationToken ct)
        {
            logger.LogInformation("Queuing NotificationSettingsRequests");
            DateTime extendedStartDateTime = StartDateTime.AddMinutes(-5);

            int iteration = 0;
            IList<UserProfile> profiles;
            do
            {
                profiles = await dbContext.UserProfile
                    .AsNoTracking()
                    .Where(p => p.CreatedDateTime > extendedStartDateTime && p.CreatedDateTime < CurrentDateTime)
                    .Skip(iteration * this.batchSize)
                    .Take(this.batchSize)
                    .Include(p => p.Verifications)
                    .ToListAsync(ct);

                foreach (UserProfile profile in profiles)
                {
                    MessagingVerification? lastSmsVerification = profile.Verifications
                        .Where(v => v.VerificationType == MessagingVerificationType.Sms)
                        .MaxBy(v => v.UpdatedDateTime);

                    NotificationSettingsRequest request = new(profile, profile.Email, profile.SmsNumber);
                    if (string.IsNullOrEmpty(profile.SmsNumber) && lastSmsVerification is { Validated: false, Deleted: false })
                    {
                        request.SmsVerificationCode = lastSmsVerification.SmsValidationCode;
                    }

                    await this.QueueNotificationSettingsRequestAsync(request, ct);
                }

                logger.LogInformation("Queued NotificationSettingsRequests for {Count} profiles", profiles.Count);
                iteration++;
            }
            while (profiles.Count == this.batchSize);
        }

        private async Task SendAccountCreatedEventAsync(string hdid, DateTime createdDateTime, CancellationToken ct)
        {
            logger.LogInformation("Sending AccountCreatedEvent for {Hdid}", hdid);
            await messageSender.SendAsync([new MessageEnvelope(new AccountCreatedEvent(hdid, createdDateTime), hdid)], ct);
        }

        private async Task QueueNotificationSettingsRequestAsync(NotificationSettingsRequest request, CancellationToken ct)
        {
            logger.LogInformation("Queuing NotificationSettingsRequest for {Hdid}", request.SubjectHdid);
            await notificationSettingsService.QueueNotificationSettingsAsync(request, ct);
        }
    }
}
