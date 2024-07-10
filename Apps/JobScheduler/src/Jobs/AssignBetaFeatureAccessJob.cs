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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Assigns users access to a beta feature.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="logger">The logger to use.</param>
    /// <param name="dbContext">The database context to use.</param>
    public class AssignBetaFeatureAccessJob(IConfiguration configuration, ILogger<AssignBetaFeatureAccessJob> logger, GatewayDbContext dbContext)
    {
        /// <summary>
        /// Key used to identify this job in the config.
        /// </summary>
        public const string JobKey = "AssignBetaFeatureAccess";

        private const string EnabledTemplateKey = "Enabled";
        private const string MaxBatchSizeKey = "MaxBatchSize";
        private const string UserCountKey = "UserCount";
        private const string BetaFeatureKey = "BetaFeature";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private readonly bool enabled = configuration.GetValue($"{JobKey}:{EnabledTemplateKey}", false);
        private readonly int maxBatchSize = configuration.GetValue($"{JobKey}:{MaxBatchSizeKey}", 0);
        private readonly int userCount = configuration.GetValue($"{JobKey}:{UserCountKey}", 0);
        private readonly BetaFeature betaFeature = configuration.GetValue<BetaFeature?>($"{JobKey}:{BetaFeatureKey}", null)
                                                   ?? throw new ArgumentNullException(nameof(configuration), $"{JobKey}:{BetaFeatureKey} is null");

        /// <summary>
        /// Assigns users access to a beta feature.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task ProcessAsync(CancellationToken ct = default)
        {
            if (!this.enabled)
            {
                logger.LogInformation("{JobKey} is disabled", JobKey);
                return;
            }

            if (this.userCount < 1)
            {
                logger.LogInformation("{JobKey}:{UserCountKey} must be at least 1", JobKey, UserCountKey);
                return;
            }

            if (this.maxBatchSize < 1)
            {
                logger.LogInformation("{JobKey}:{MaxBatchSizeKey} must be at least 1", JobKey, MaxBatchSizeKey);
                return;
            }

            logger.LogInformation("Starting process of assigning access to {BetaFeature} beta feature for up to {UserCount} user(s)", EnumUtility.ToEnumString(this.betaFeature), this.userCount);
            int processedUserCount = 0;
            while (processedUserCount < this.userCount)
            {
                int batchSize = Math.Min(this.maxBatchSize, this.userCount - processedUserCount);

                logger.LogInformation("Retrieving up to {BatchSize} user(s) with verified email addresses who have recently logged in", batchSize);
                IList<UserProfile> userProfiles = await this.GetRecentUserProfiles(processedUserCount, batchSize, ct);

                int retrievedCount = userProfiles.Count;
                logger.LogInformation("Retrieved {RetrievedCount} user(s)", retrievedCount);
                if (retrievedCount > 0)
                {
                    logger.LogInformation("Adding beta feature access for retrieved user(s)");
                    IEnumerable<BetaFeatureAccess> associations = userProfiles.Select(this.GenerateBetaFeatureAccess);
                    dbContext.BetaFeatureAccess.AddRange(associations);
                    await dbContext.SaveChangesAsync(ct); // commit after each iteration
                }

                processedUserCount += retrievedCount;
                if (retrievedCount < batchSize)
                {
                    break;
                }
            }

            logger.LogInformation("Completed processing, having assigned beta feature access to {ProcessedUserCount} user(s)", processedUserCount);
        }

        private async Task<IList<UserProfile>> GetRecentUserProfiles(int skip, int take, CancellationToken ct)
        {
            return await dbContext.UserProfile
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Where(u => u.BetaFeatureCodes.All(c => c.Code != this.betaFeature))
                .OrderByDescending(o => o.LastLoginDateTime)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        private BetaFeatureAccess GenerateBetaFeatureAccess(UserProfile userProfile)
        {
            DateTime now = DateTime.UtcNow;
            return new BetaFeatureAccess
            {
                Hdid = userProfile.HdId,
                BetaFeatureCode = this.betaFeature,
                CreatedDateTime = now,
                UpdatedDateTime = now,
            };
        }
    }
}
