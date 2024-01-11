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
namespace HealthGateway.JobScheduler.Listeners
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// A background listener that monitors the Audit queue and writes them to the database.
    /// Actions that may come are DELETE, UPDATE, and INSERT.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuditQueueListener : BackgroundService
    {
        private const int SleepDuration = 1000;
        private readonly ILogger<AuditQueueListener> logger;
        private readonly IServiceProvider services;
        private readonly IConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditQueueListener"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="services">The set of application services available.</param>
        /// <param name="connectionMultiplexer">The injected connection multiplexer.</param>
        public AuditQueueListener(
            ILogger<AuditQueueListener> logger,
            IServiceProvider services,
            IConnectionMultiplexer connectionMultiplexer)
        {
            this.logger = logger;
            this.services = services;
            this.connectionMultiplexer = connectionMultiplexer;
        }

        /// <summary>
        /// Listens for Audit Events and writes them to the DB.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Audit Queue Listener is starting");
            stoppingToken.Register(() => this.logger.LogInformation("AuditQueue Listener Shutdown as cancellation requested    "));
            while (!stoppingToken.IsCancellationRequested)
            {
                IDatabase redisDb = this.connectionMultiplexer.GetDatabase();
                RedisValue auditValue = await redisDb.ListMoveAsync(
                    $"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ActiveQueueName}",
                    $"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ProcessingQueueName}",
                    ListSide.Left,
                    ListSide.Right);
                if (auditValue.HasValue)
                {
                    await this.ProcessAuditEventAsync(redisDb, auditValue, stoppingToken);
                }
                else
                {
                    await Task.Delay(SleepDuration, stoppingToken);
                }
            }

            this.logger.LogInformation("Audit Queue Listener has stopped");
        }

        private async Task ProcessAuditEventAsync(IDatabase redisDb, RedisValue auditValue, CancellationToken ct)
        {
            this.logger.LogTrace("Start Processing Audit Event...");
            AuditEvent? auditEvent = JsonSerializer.Deserialize<AuditEvent>(auditValue.ToString());
            if (auditEvent != null)
            {
                try
                {
                    using IServiceScope scope = this.services.CreateScope();
                    IWriteAuditEventDelegate writeAuditEventDelegate = scope.ServiceProvider.GetRequiredService<IWriteAuditEventDelegate>();
                    await writeAuditEventDelegate.WriteAuditEventAsync(auditEvent, ct);
                    await redisDb.ListRemoveAsync($"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ProcessingQueueName}", auditValue);
                }
                catch (DataException e)
                {
                    this.logger.LogError("Error writing to DB:\n{@Exception}", e);
                }
            }

            this.logger.LogTrace("Completed Audit Event Processing...");
        }
    }
}
