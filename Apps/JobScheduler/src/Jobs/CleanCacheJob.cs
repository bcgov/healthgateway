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
    using System.Linq;
    using Hangfire;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Removes expired cache entries from the GenericCache table.
    /// </summary>
    public class CleanCacheJob
    {
        private const string JobKey = "CleanCache";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private readonly ILogger<CleanCacheJob> logger;
        private readonly GatewayDbContext dbContext;
        private readonly int deleteMaxRows;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanCacheJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        public CleanCacheJob(
            IConfiguration configuration,
            ILogger<CleanCacheJob> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            IConfigurationSection jobConfig = configuration.GetSection($"{JobKey}");
            this.deleteMaxRows = jobConfig.GetValue("DeleteMaxRows", 1000);
        }

        /// <summary>
        /// Reads the configuration and will instantiate and run the class a single time.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            this.logger.LogInformation("CleanCacheJob Starting");
            List<GenericCache> oldIds = this.dbContext.GenericCache
                .Where(cache => cache.ExpiryDateTime < DateTime.UtcNow)
                .Select(cache => new GenericCache { Id = cache.Id, Version = cache.Version, ExpiryDateTime = cache.ExpiryDateTime })
                .OrderBy(o => o.ExpiryDateTime)
                .Take(this.deleteMaxRows)
                .ToList();
            if (oldIds.Count > 0)
            {
                this.logger.LogInformation($"Deleting {oldIds.Count} Generic cache entries out of a maximum of {this.deleteMaxRows}");
                this.dbContext.RemoveRange(oldIds);
                this.dbContext.SaveChanges();
            }

            this.logger.LogInformation("CleanCacheJob Finished running");
        }
    }
}
