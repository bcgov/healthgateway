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
        /// <summary>
        /// The queue name where we store AuditEvents while processing them.
        /// </summary>
        public const string AuditQueueProcessing = "Queue:Audit:Processing";

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
        /// <returns>The task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Audit Queue Listener is starting");
            while (!stoppingToken.IsCancellationRequested)
            {
                IDatabase redisDb = this.connectionMultiplexer.GetDatabase();
                RedisValue auditValue = await redisDb.ListMoveAsync(
                        RedisAuditLogger.AuditQueue,
                        AuditQueueProcessing,
                        ListSide.Left,
                        ListSide.Right,
                        CommandFlags.None)
                    .ConfigureAwait(true);
                if (auditValue.HasValue)
                {
                    processAuditEvent(redisDb, auditValue);
                }
                else
                {
                    await Task.Delay(SleepDuration, stoppingToken).ConfigureAwait(true);
                }
            }

            this.logger.LogInformation("Audit Queue Listener has stopped");
        }

        private async Task processAuditEvent(IDatabase redisDb, RedisValue auditValue)
        {
            AuditEvent? auditEvent = JsonSerializer.Deserialize<AuditEvent>(auditValue.ToString());
            if (auditEvent != null)
            {
                try
                {
                    using IServiceScope scope = this.services.CreateScope();
                    IWriteAuditEventDelegate writeAuditEventDelegate = scope.ServiceProvider.GetRequiredService<IWriteAuditEventDelegate>();
                    writeAuditEventDelegate.WriteAuditEvent(auditEvent);
                    await redisDb.ListRemoveAsync(AuditQueueProcessing, auditValue).ConfigureAwait(true);
                }
                catch (DataException e)
                {
                    this.logger.LogError($"Error writing to DB:\n{e.StackTrace}");
                }
            }
        }
    }
}
