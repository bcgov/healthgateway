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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.JobScheduler.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Backfills notification settings for eligible users.
    /// </summary>
    /// <param name="dbContext">The dbContext to use.</param>
    /// <param name="outboxStore">The outbox store to use.</param>
    /// <param name="backgroundJobClient">Hangfire background job client.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="optionsMonitor">The monitor used to access the current notification backfill options.</param>
    public class NotificationBackfillJob(
        GatewayDbContext dbContext,
        IOutboxStore outboxStore,
        IBackgroundJobClient backgroundJobClient,
        ILogger<NotificationBackfillJob> logger,
        IOptionsMonitor<NotificationBackfillOptions> optionsMonitor)
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private NotificationBackfillOptions options = null!;

        /// <summary>
        /// Processes user profiles in batches to pre-populate notification settings for a specific notification type.
        /// Uses UserJobState tracking to ensure users are processed only once per job.
        /// Each batch is processed transactionally, and corresponding notification events
        /// are generated and dispatched via the outbox after a successful commit.
        /// </summary>
        /// <param name="optionsName">The name of the configuration section used to bind the job options.</param>
        /// <param name="ct">Cancellation token used to stop processing gracefully.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task ProcessAsync(string optionsName, CancellationToken ct = default)
        {
            this.options = optionsMonitor.Get(optionsName);

            // Skip execution if the job is disabled via configuration
            if (!this.options.Enabled)
            {
                logger.LogInformation("Job {JobName} is disabled", this.options.JobName);
                return;
            }

            logger.LogInformation("Job {JobName} started", this.options.JobName);
            Stopwatch stopwatch = Stopwatch.StartNew();

            ProfileNotificationType notificationType =
                Enum.Parse<ProfileNotificationType>(this.options.NotificationType);

            try
            {
                // Gets user profile that require notification settings to be created/updated
                List<UserProfile> userProfilesToBackfill = await this.GetNextUserProfilesAsync(notificationType, ct);

                await this.UpsertUserProfileNotificationSettingsAsync(notificationType, userProfilesToBackfill, ct);

                stopwatch.Stop();

                logger.LogInformation(
                    "Job {JobName} finished in {ElapsedMs} ms for {Count} profiles",
                    this.options.JobName,
                    stopwatch.ElapsedMilliseconds,
                    userProfilesToBackfill.Count);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw new InvalidOperationException(
                    $"Job {this.options.JobName} run failed after {stopwatch.ElapsedMilliseconds} ms.",
                    ex);
            }
        }

        /// <summary>
        /// Determines the notification targets for a given notification type and channel.
        /// Returns a target only when the channel is enabled and the user has a valid
        /// contact value; otherwise returns an empty collection.
        /// </summary>
        /// <param name="type">The notification type being processed.</param>
        /// <param name="enabled">Indicates whether the channel is enabled.</param>
        /// <param name="hasChannelValue">
        /// Indicates whether the user has a valid value for the channel (e.g., email or SMS).
        /// </param>
        /// <returns>
        /// A collection containing the resolved <see cref="NotificationTargets"/>, or empty if not applicable.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the notification type is not supported.
        /// </exception>
        private static IReadOnlyCollection<NotificationTargets> GetTargets(
            ProfileNotificationType type,
            bool enabled,
            bool hasChannelValue)
        {
            if (!enabled || !hasChannelValue)
            {
                return [];
            }

            NotificationTargets target = type switch
            {
                ProfileNotificationType.BcCancerScreening => NotificationTargets.BcCancer,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported notification type"),
            };

            return [target];
        }

        /// <summary>
        /// Creates a notification event message for a user profile based on configured channel preferences.
        /// Email and SMS targets are only included when corresponding configuration values are provided.
        /// Targets are further filtered based on whether the user has valid contact information.
        /// </summary>
        /// <param name="notificationType">The notification type being processed.</param>
        /// <param name="userProfile">The user profile for which the event is created.</param>
        /// <returns>A <see cref="MessageEnvelope"/> containing the notification event.</returns>
        private MessageEnvelope CreateMessageEnvelope(
            ProfileNotificationType notificationType,
            UserProfile userProfile)
        {
            // Build email targets only when EmailEnabled is configured
            IReadOnlyCollection<NotificationTargets> emailNotificationTargets =
                this.options.EmailEnabled.HasValue
                    ? GetTargets(
                        notificationType,
                        this.options.EmailEnabled.Value,
                        !string.IsNullOrEmpty(userProfile.Email))
                    : [];

            // Build SMS targets only when SmsEnabled is configured
            IReadOnlyCollection<NotificationTargets> smsNotificationTargets =
                this.options.SmsEnabled.HasValue
                    ? GetTargets(
                        notificationType,
                        this.options.SmsEnabled.Value,
                        !string.IsNullOrEmpty(userProfile.SmsNumber))
                    : [];

            // Create event wrapped in message envelope for outbox processing
            return new MessageEnvelope(
                new NotificationChannelPreferencesChangedEvent(
                    userProfile.HdId,
                    userProfile.SmsNumber,
                    smsNotificationTargets,
                    userProfile.Email,
                    emailNotificationTargets),
                userProfile.HdId);
        }

        /// <summary>
        /// Creates a new <see cref="UserProfileNotificationSetting"/> for a user profile,
        /// initializing channel values from configured defaults.
        /// </summary>
        /// <param name="notificationType">The notification type being created.</param>
        /// <param name="hdid">The user profile HDID.</param>
        /// <returns>A new <see cref="UserProfileNotificationSetting"/> instance.</returns>
        private UserProfileNotificationSetting CreateNotificationSetting(
            ProfileNotificationType notificationType,
            string hdid)
        {
            return new UserProfileNotificationSetting
            {
                Hdid = hdid,
                NotificationType = notificationType,
                EmailEnabled = this.options.EmailEnabled,
                SmsEnabled = this.options.SmsEnabled,
            };
        }

        /// <summary>
        /// Creates a <see cref="UserJobState"/> record to track that a user profile
        /// has been processed by the current job run.
        /// </summary>
        /// <param name="hdid">The user profile HDID.</param>
        /// <param name="processedDateTime">The UTC date/time the user was processed.</param>
        /// <remarks>
        /// JobName is used as a logical identifier for the job in UserJobState tracking.
        /// It must remain consistent for a given backfill job to prevent reprocessing.
        /// Changing JobName will cause all users to be treated as unprocessed.
        /// </remarks>
        /// <returns>A new <see cref="UserJobState"/> instance.</returns>
        private UserJobState CreateUserJobState(string hdid, DateTime processedDateTime)
        {
            return new UserJobState
            {
                JobName = this.options.JobName,
                Hdid = hdid,
                ProcessedDateTime = processedDateTime,
            };
        }

        /// <summary>
        /// Retrieves existing notification settings for the specified HDIDs and notification type,
        /// and returns them as a dictionary keyed by HDID for efficient lookup.
        /// </summary>
        /// <param name="notificationType">The notification type being processed.</param>
        /// <param name="hdids">The list of user profile HDIDs to retrieve settings for.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A dictionary of <see cref="UserProfileNotificationSetting"/> keyed by HDID.
        /// </returns>
        private async Task<Dictionary<string, UserProfileNotificationSetting>> GetExistingSettingsByHdidAsync(
            ProfileNotificationType notificationType,
            List<string> hdids,
            CancellationToken ct)
        {
            List<UserProfileNotificationSetting> existingSettings = await dbContext.UserProfileNotificationSetting
                .Where(ns =>
                    hdids.Contains(ns.Hdid) &&
                    ns.NotificationType == notificationType)
                .ToListAsync(ct);

            return existingSettings.ToDictionary(x => x.Hdid);
        }

        /// <summary>
        /// Retrieves the next batch of user profiles eligible for notification backfill.
        /// Selection criteria:
        /// - Channel is determined by <c>UseSmsChannel</c>:
        ///   - When false: selects users with a non-empty Email
        ///   - When true: selects users with a non-empty SmsNumber
        /// - No existing notification setting for the specified type, or only null values for the selected channel:
        ///   - When false: EmailEnabled is null
        ///   - When true: SmsEnabled is null
        /// - User has not already been processed for this job (no matching UserJobState for Hdid and JobName)
        /// Results are ordered by most recent login (descending) with HDID as a tie-breaker
        /// to ensure deterministic batching.
        /// </summary>
        /// <param name="notificationType">The notification type being processed.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A list of user profiles to process in the next batch.</returns>
        private async Task<List<UserProfile>> GetNextUserProfilesAsync(ProfileNotificationType notificationType, CancellationToken ct)
        {
            IQueryable<UserProfile> query = dbContext.UserProfile
                .AsNoTracking()
                .Where(x =>
                    this.options.UseSmsChannel
                        ? !string.IsNullOrWhiteSpace(x.SmsNumber)
                        : !string.IsNullOrWhiteSpace(x.Email))
                .Where(x =>
                    dbContext.UserProfileNotificationSetting
                        .Where(ns =>
                            ns.Hdid == x.HdId &&
                            ns.NotificationType == notificationType)
                        .All(ns =>
                            this.options.UseSmsChannel
                                ? ns.SmsEnabled == null
                                : ns.EmailEnabled == null))
                .Where(x =>
                    !dbContext.UserJobState.Any(js =>
                        js.Hdid == x.HdId &&
                        js.JobName == this.options.JobName))
                .OrderByDescending(x => x.LastLoginDateTime)
                .ThenBy(x => x.HdId);

            return await query
                .Take(this.options.BatchSize)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Persists batch results to the database, including new notification settings,
        /// job state records, and outbound events via the transactional outbox.
        /// All operations are expected to run within an existing transaction.
        /// </summary>
        /// <param name="result">The batch result containing inserts, job states, and events.</param>
        /// <param name="ct">The cancellation token.</param>
        private async Task PersistBatchAsync(NotificationBackfillBatchResult result, CancellationToken ct)
        {
            // Insert new notification settings (if any)
            if (result.RowsToInsert.Count != 0)
            {
                await dbContext.UserProfileNotificationSetting.AddRangeAsync(result.RowsToInsert, ct);
            }

            // Insert job state records for tracking processed users
            await dbContext.UserJobState.AddRangeAsync(result.UserJobStates, ct);

            // Persist events to the outbox for later dispatch (if any)
            if (result.Events.Count != 0)
            {
                await outboxStore.StoreAsync(result.Events, false, ct);
            }
        }

        /// <summary>
        /// Processes a batch of user profiles to determine notification setting inserts,
        /// updates (only for null values), job state records, and outbound events.
        /// For each user:
        /// - Updates existing settings only when values are null (does not override user choices)
        /// - Creates a new setting when none exists
        /// - Generates a corresponding job state and notification event.
        /// </summary>
        /// <param name="notificationType">The notification type being processed.</param>
        /// <param name="userProfilesToBackfill">The batch of user profiles to process.</param>
        /// <param name="existingSettingsByHdid">
        /// Existing notification settings keyed by HDID for efficient lookup.
        /// </param>
        /// <returns>
        /// A <see cref="NotificationBackfillBatchResult"/> containing inserts, updates,
        /// job states, and events for the batch.
        /// </returns>
        private NotificationBackfillBatchResult ProcessBatch(
            ProfileNotificationType notificationType,
            List<UserProfile> userProfilesToBackfill,
            Dictionary<string, UserProfileNotificationSetting> existingSettingsByHdid)
        {
            DateTime processedDateTime = DateTime.UtcNow;

            List<UserProfileNotificationSetting> notificationSettingsToInsert = [];
            List<UserJobState> jobStatesToInsert = [];
            List<MessageEnvelope> notificationEvents = [];
            int updatedCount = 0;

            foreach (UserProfile userProfile in userProfilesToBackfill)
            {
                string hdid = userProfile.HdId;

                if (existingSettingsByHdid.TryGetValue(hdid, out UserProfileNotificationSetting? existing))
                {
                    // Update existing setting only if eligible (e.g., null values); track if changed
                    if (this.UpdateExistingSetting(existing))
                    {
                        updatedCount++;
                    }
                }
                else
                {
                    // Create new setting when none exists for this user
                    notificationSettingsToInsert.Add(this.CreateNotificationSetting(notificationType, hdid));
                }

                jobStatesToInsert.Add(this.CreateUserJobState(hdid, processedDateTime));
                notificationEvents.Add(this.CreateMessageEnvelope(notificationType, userProfile));
            }

            return new NotificationBackfillBatchResult(
                notificationSettingsToInsert,
                jobStatesToInsert,
                notificationEvents,
                updatedCount);
        }

        /// <summary>
        /// Updates an existing notification setting by populating null channel values
        /// (EmailEnabled and/or SmsEnabled) using configured defaults.
        /// Does not override explicitly set user preferences.
        /// </summary>
        /// <param name="existing">The existing notification setting to evaluate and update.</param>
        /// <returns>
        /// True if at least one field was updated; otherwise, false.
        /// </returns>
        private bool UpdateExistingSetting(UserProfileNotificationSetting existing)
        {
            bool updated = false;

            if (existing.EmailEnabled == null && this.options.EmailEnabled.HasValue)
            {
                existing.EmailEnabled = this.options.EmailEnabled.Value;
                updated = true;
            }

            if (existing.SmsEnabled == null && this.options.SmsEnabled.HasValue)
            {
                existing.SmsEnabled = this.options.SmsEnabled.Value;
                updated = true;
            }

            return updated;
        }

        /// <summary>
        /// Upserts notification settings for a batch of user profiles.
        /// For each user:
        /// - Updates existing settings only when channel values are null (does not override user choices)
        /// - Creates new settings when none exist
        /// All changes are persisted within a single transaction, and any generated events
        /// are dispatched after a successful commit.
        /// </summary>
        /// <param name="notificationType">The notification type being processed.</param>
        /// <param name="userProfilesToBackfill">The batch of user profiles to process.</param>
        /// <param name="ct">The cancellation token.</param>
        private async Task UpsertUserProfileNotificationSettingsAsync(
            ProfileNotificationType notificationType,
            List<UserProfile> userProfilesToBackfill,
            CancellationToken ct)
        {
            if (userProfilesToBackfill.Count == 0)
            {
                logger.LogInformation("Job {JobName} no records to process", this.options.JobName);
                return;
            }

            // Begin transaction to ensure atomic batch processing
            await using IDbContextTransaction transaction =
                await dbContext.Database.BeginTransactionAsync(ct);

            // Extract HDIDs for lookup
            List<string> hdids = [.. userProfilesToBackfill.Select(x => x.HdId)];

            // Retrieve existing settings keyed by HDID for efficient access
            Dictionary<string, UserProfileNotificationSetting> existingSettingsByHdid =
                await this.GetExistingSettingsByHdidAsync(notificationType, hdids, ct);

            // Determine inserts, updates, and events for this batch
            NotificationBackfillBatchResult result = this.ProcessBatch(
                notificationType,
                userProfilesToBackfill,
                existingSettingsByHdid);

            // Persist new settings and enqueue outbox events
            await this.PersistBatchAsync(result, ct);

            // Save all changes within the transaction
            await dbContext.SaveChangesAsync(ct);

            // Commit transaction after successful persistence
            await transaction.CommitAsync(ct);

            // Dispatch events after commit (outbox pattern)
            if (result.Events.Count != 0)
            {
                logger.LogDebug("Dispatching events after commit");
                backgroundJobClient.Enqueue<DbOutboxStore>(store =>
                    store.DispatchOutboxItemsAsync(CancellationToken.None));
            }

            // Log batch summary
            logger.LogInformation(
                "Job {JobName} processed {TotalProfiles} profiles. Inserted {InsertCount}. Updated {UpdateCount}",
                this.options.JobName,
                userProfilesToBackfill.Count,
                result.RowsToInsert.Count,
                result.UpdatedCount);
        }

        private sealed record NotificationBackfillBatchResult(
            List<UserProfileNotificationSetting> RowsToInsert,
            List<UserJobState> UserJobStates,
            List<MessageEnvelope> Events,
            int UpdatedCount);
    }
}
