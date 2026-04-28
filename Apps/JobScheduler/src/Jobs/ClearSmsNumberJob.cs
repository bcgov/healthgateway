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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.JobScheduler.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Executes the SMS cleanup workflow by clearing SmsNumber for eligible user profiles.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="clearSmsNumberOptionsMonitor">Provides access to the configured clear SMS number job options.</param>
    /// <param name="notificationBackfillOptionsMonitor">Provides access to the configured notification backfill job options.</param>
    public class ClearSmsNumberJob(
        GatewayDbContext dbContext,
        ILogger<ClearSmsNumberJob> logger,
        IOptionsMonitor<ClearSmsNumberOptions> clearSmsNumberOptionsMonitor,
        IOptionsMonitor<NotificationBackfillOptions> notificationBackfillOptionsMonitor)
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private const string ClearSmsNumberOptionsName = "ClearSmsNumber";
        private const string NotificationSmsBackfillOptionsName = "NotificationSmsBackfill";

        private ClearSmsNumberOptions clearSmsNumberOptions = null!;
        private NotificationBackfillOptions notificationBackfillOptions = null!;

        /// <summary>
        /// Executes the SMS cleanup workflow by clearing SmsNumber for eligible user profiles.
        /// </summary>
        /// <param name="ct">Cancellation token used to stop processing gracefully.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task ProcessAsync(CancellationToken ct = default)
        {
            this.clearSmsNumberOptions = clearSmsNumberOptionsMonitor.Get(ClearSmsNumberOptionsName);
            this.ValidateCutoffMatchesNotificationBackfill();
            await this.ClearSmsNumbersAsync(ct);
        }

        /// <summary>
        /// Creates a <see cref="UserPreference"/> record for the specified user.
        /// </summary>
        /// <param name="hdid">The user profile HDID.</param>
        /// <param name="preference">The preference key.</param>
        /// <param name="value">The preference value.</param>
        /// <returns>A new <see cref="UserPreference"/> instance.</returns>
        private static UserPreference CreateUserPreference(string hdid, string preference, string value)
        {
            return new()
            {
                HdId = hdid,
                Preference = preference,
                Value = value,
            };
        }

        /// <summary>
        /// Creates a <see cref="UserJobState"/> record to track that a user profile
        /// has been processed by the current job run.
        /// </summary>
        /// <param name="hdid">The user profile HDID.</param>
        /// <param name="jobName">Logical job identifier (must match configured job name).</param>
        /// <param name="processedDateTime">The UTC date/time the user was processed.</param>
        /// <remarks>
        /// JobName is used as a logical identifier for the job in UserJobState tracking.
        /// It must remain consistent for a given job to prevent reprocessing.
        /// Changing JobName will cause all users to be treated as unprocessed.
        /// </remarks>
        /// <returns>A new <see cref="UserJobState"/> instance.</returns>
        private static UserJobState CreateUserJobState(string hdid, string jobName, DateTime processedDateTime)
        {
            return new()
            {
                JobName = jobName,
                Hdid = hdid,
                ProcessedDateTime = processedDateTime,
            };
        }

        /// <summary>
        /// Validates that the cutoff date configured for the ClearSmsNumber job
        /// matches the cutoff date configured for the NotificationSmsBackfill job.
        /// </summary>
        /// <remarks>
        /// These two jobs are designed to operate on mutually exclusive sets of users
        /// based on LastLoginDateTime:
        /// - ClearSmsNumber processes users with LastLoginDateTime less than the cutoff
        /// - NotificationSmsBackfill processes users with LastLoginDateTime on or after the cutoff
        /// If the cutoff dates differ, the partitioning becomes invalid and can result in:
        /// - Overlap: the same user being processed by both jobs
        /// - Gaps: some users not being processed by either job
        /// This validation ensures configuration consistency and fails fast if the cutoff
        /// dates are misaligned.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the cutoff dates for the two jobs do not match.
        /// </exception>
        private void ValidateCutoffMatchesNotificationBackfill()
        {
            this.notificationBackfillOptions = notificationBackfillOptionsMonitor.Get(NotificationSmsBackfillOptionsName);
            DateTime? clearSmsNumberCutoff = this.clearSmsNumberOptions.LastLoginAfterDate?.ToUniversalTime();
            DateTime? notificationBackfillCutoff = this.notificationBackfillOptions.LastLoginAfterDate?.ToUniversalTime();

            if (clearSmsNumberCutoff != notificationBackfillCutoff)
            {
                throw new InvalidOperationException(
                    $"Cutoff mismatch: {ClearSmsNumberOptionsName} ({clearSmsNumberCutoff:o}) " +
                    $"!= {NotificationSmsBackfillOptionsName} ({notificationBackfillCutoff:o}).");
            }
        }

        private async Task ClearSmsNumbersAsync(CancellationToken ct)
        {
            // Skip execution if the job is disabled via configuration
            if (!this.clearSmsNumberOptions.Enabled)
            {
                logger.LogInformation("Job {JobName} is disabled", this.clearSmsNumberOptions.JobName);
                return;
            }

            DateTime cutoffDate = this.clearSmsNumberOptions.LastLoginAfterDate?.ToUniversalTime()
                                  ?? throw new InvalidOperationException("LastLoginAfterDate must be configured for ClearSmsNumberJob");

            logger.LogInformation("Job {JobName} started", this.clearSmsNumberOptions.JobName);
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                List<UserProfile> userProfiles = await this.GetNextUserProfilesBeforeCutoffAsync(
                    cutoffDate,
                    this.clearSmsNumberOptions.JobName,
                    this.clearSmsNumberOptions.BatchSize,
                    ct);

                if (userProfiles.Count > 0)
                {
                    // Begin transaction to ensure atomic batch processing
                    await using IDbContextTransaction transaction =
                        await dbContext.Database.BeginTransactionAsync(ct);

                    DateTime processedDateTime = DateTime.UtcNow;
                    List<UserJobState> jobStatesToInsert = [];
                    List<UserPreference> preferencesToInsert = [];

                    foreach (UserProfile userProfile in userProfiles)
                    {
                        userProfile.SmsNumber = null;
                        jobStatesToInsert.Add(CreateUserJobState(userProfile.HdId, this.clearSmsNumberOptions.JobName, processedDateTime));
                        preferencesToInsert.Add(CreateUserPreference(userProfile.HdId, "showSmsRemoved", "true"));
                    }

                    dbContext.UserJobState.AddRange(jobStatesToInsert);
                    dbContext.UserPreference.AddRange(preferencesToInsert);

                    await dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }

                stopwatch.Stop();

                logger.LogInformation(
                    "Job {JobName} cleared SMS numbers for {Count} profiles in {ElapsedMs} ms",
                    this.clearSmsNumberOptions.JobName,
                    userProfiles.Count,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw new InvalidOperationException(
                    $"Job {this.clearSmsNumberOptions.JobName} run failed after {stopwatch.ElapsedMilliseconds} ms.",
                    ex);
            }
        }

        /// <summary>
        /// Retrieves the next batch of user profiles with SmsNumber set and LastLoginDateTime before the configured cutoff.
        /// </summary>
        /// <param name="lastLoginCutoffDate">The UTC cutoff date used to filter users by LastLoginDateTime.</param>
        /// <param name="jobName">Logical job identifier (must match configured job name).</param>
        /// <param name="batchSize">Maximum number of users to process in a single batch.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A list of user profiles to process in the next batch.</returns>
        private async Task<List<UserProfile>> GetNextUserProfilesBeforeCutoffAsync(
            DateTime lastLoginCutoffDate,
            string jobName,
            int batchSize,
            CancellationToken ct)
        {
            DateTime cutoffDate = lastLoginCutoffDate.ToUniversalTime();
            return await dbContext.UserProfile
                .Where(x => !string.IsNullOrWhiteSpace(x.SmsNumber))
                .Where(x => x.LastLoginDateTime < cutoffDate)
                .Where(x =>
                    !dbContext.UserJobState.Any(js =>
                        js.Hdid == x.HdId &&
                        js.JobName == jobName))
                .OrderByDescending(x => x.LastLoginDateTime)
                .ThenBy(x => x.HdId)
                .Take(batchSize)
                .ToListAsync(ct);
        }
    }
}
