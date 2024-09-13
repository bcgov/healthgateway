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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// A background listener that monitors the Audit queue and writes them to the database.
    /// Actions that may come are DELETE, UPDATE, and INSERT.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="services">The set of application services available.</param>
    /// <param name="connectionMultiplexer">The injected connection multiplexer.</param>
    /// <param name="configuration">The configuration to use.</param>
    [ExcludeFromCodeCoverage]
    public class AuditQueueListener(
        ILogger<AuditQueueListener> logger,
        IServiceProvider services,
        IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration) : BackgroundService
    {
        private const string ExponentialBaseKey = "ExponentialBase";
        private const string ListenerKey = "AuditQueueListener";
        private const string MaxBackoffDelayKey = "MaxBackoffDelay";
        private const string RetryDelayKey = "RetryDelay";
        private const string SleepDurationKey = "SleepDuration";

        private readonly int exponentialBase = configuration.GetValue($"{ListenerKey}:{ExponentialBaseKey}", 2);
        private readonly int maxBackoffDelay = configuration.GetValue($"{ListenerKey}:{MaxBackoffDelayKey}", 16000); // Set the default max backoff delay to 16 seconds
        private readonly int retryDelay = configuration.GetValue($"{ListenerKey}:{RetryDelayKey}", 1000); // Used with exponentialBase and maxBackoffDelay to determine backoffDelay
        private readonly int sleepDuration = configuration.GetValue($"{ListenerKey}:{SleepDurationKey}", 1000);

        /// <summary>
        /// Listens for Audit Events and writes them to the DB.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Audit Queue Listener is starting");
            stoppingToken.Register(() => logger.LogInformation("Audit Queue Listener Shutdown as cancellation requested"));

            int retryCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IDatabase redisDb = connectionMultiplexer.GetDatabase();
                    RedisValue auditValue = await redisDb.ListMoveAsync(
                        $"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ActiveQueueName}",
                        $"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ProcessingQueueName}",
                        ListSide.Left,
                        ListSide.Right);

                    if (auditValue.HasValue)
                    {
                        await this.ProcessAuditEventAsync(redisDb, auditValue, stoppingToken);
                        retryCount = 0; // Reset retry count on successful processing
                    }
                    else
                    {
                        await Task.Delay(this.sleepDuration, stoppingToken);
                    }
                }
                catch (RedisConnectionException ex)
                {
                    logger.LogError(ex, "Redis connection error occurred in Audit Queue Listener. Retrying after delay");
                    retryCount++;
                    await this.HandleExceptionAsync(retryCount, stoppingToken);
                }
                catch (InvalidOperationException ex)
                {
                    // Occurs when a stale object is unsuccessfully saved from the UI
                    logger.LogError(ex, "Database error occurred in Audit Queue Listener. Retrying after delay");
                    retryCount++;
                    await this.HandleExceptionAsync(retryCount, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error in Audit Queue Listener. Retrying after delay");
                    retryCount++;
                    await this.HandleExceptionAsync(retryCount, stoppingToken);
                }
            }

            logger.LogInformation("Audit Queue Listener has stopped");
        }

        private async Task HandleExceptionAsync(int retryCount, CancellationToken stoppingToken)
        {
            // Exponential backoff with a cap where exponentialBase is 2, maxBackoffDelay is 16000ms and retryDelay is 1000ms.
            // Retry 1: 2 seconds
            // Retry 2: 4 seconds
            // Retry 3: 8 seconds
            // Retry 4: 16 seconds
            // Retry 5: 16 seconds (maxRetryDelay at 16 seconds if applied)
            int backoffDelay = Math.Min(this.retryDelay * (int)Math.Pow(this.exponentialBase, retryCount), this.maxBackoffDelay);
            logger.LogWarning("Retrying due to error in {BackoffDelay}ms )", backoffDelay);

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(backoffDelay, stoppingToken);
            }
        }

        private async Task ProcessAuditEventAsync(IDatabase redisDb, RedisValue auditValue, CancellationToken ct)
        {
            logger.LogTrace("Start Processing Audit Event...");
            AuditEvent? auditEvent = JsonSerializer.Deserialize<AuditEvent>(auditValue.ToString());
            if (auditEvent != null)
            {
                try
                {
                    using IServiceScope scope = services.CreateScope();
                    IWriteAuditEventDelegate writeAuditEventDelegate = scope.ServiceProvider.GetRequiredService<IWriteAuditEventDelegate>();
                    await writeAuditEventDelegate.WriteAuditEventAsync(auditEvent, ct);
                    await redisDb.ListRemoveAsync($"{RedisAuditLogger.AuditQueuePrefix}:{RedisAuditLogger.ProcessingQueueName}", auditValue);
                }
                catch (DataException e)
                {
                    logger.LogError(e, "Error writing to DB:\n{Message}", e.Message);
                }
            }

            logger.LogTrace("Completed Audit Event Processing...");
        }
    }
}
