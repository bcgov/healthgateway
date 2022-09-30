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
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Runs the Database migrations as needed.
    /// </summary>
    public class DBMigrationsJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private readonly GatewayDbContext dbContext;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBMigrationsJob"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        public DBMigrationsJob(
            ILogger<DBMigrationsJob> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Runs the Database migrations.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Migrate()
        {
            this.logger.LogTrace("Migrating database... {ConcurrencyTimeout}", ConcurrencyTimeout);
            this.dbContext.Database.Migrate();
            this.logger.LogTrace("Finished migrating database. {ConcurrencyTimeout}", ConcurrencyTimeout);
        }
    }
}
