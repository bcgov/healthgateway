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
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <summary>
    /// Runs the database migrations as needed.
    /// </summary>
    public class DbMigrationsJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes
        private readonly GatewayDbContext dbContext;
        private readonly ILogger<DbMigrationsJob> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbMigrationsJob"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        public DbMigrationsJob(ILogger<DbMigrationsJob> logger, GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Runs the Database migrations.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task MigrateAsync(CancellationToken ct = default)
        {
            const string jobName = nameof(this.MigrateAsync);

            this.logger.LogInformation(
                "Job '{JobName}' - Checking for pending database migrations",
                jobName);

            // Retrieve all migrations that are defined in the application's assembly
            IEnumerable<string> pendingMigrations = await this.dbContext.Database.GetPendingMigrationsAsync(ct);

            if (pendingMigrations.Any())
            {
                this.logger.LogInformation(
                    "Job '{JobName}' - Pending migrations found. Applying database migrations...",
                    jobName);

                // Retrieve the service provider from the current DbContext
                IServiceProvider serviceProvider = this.dbContext.GetInfrastructure();
                NpgsqlDataSource dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();

                // Create a new DbContextOptionsBuilder with the required configuration
                DbContextOptionsBuilder<GatewayDbContext> optionsBuilder = new();
                optionsBuilder
                    .UseNpgsql(
                        dataSource,
                        builder => builder.MigrationsHistoryTable("__EFMigrationsHistory", "gateway"))
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

                // Use a temporary DbContext for applying migrations
                await using (GatewayDbContext migrationDbContext = new(optionsBuilder.Options))
                {
                    await migrationDbContext.Database.MigrateAsync(ct);
                }

                this.logger.LogInformation("Applied database migrations successfully");
            }
            else
            {
                this.logger.LogInformation(
                    "Job '{JobName}' - No pending migrations found. Skipping migration step",
                    jobName);
            }
        }
    }
}
