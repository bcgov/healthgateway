//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.JobScheduler.Listeners
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Converters;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <summary>
    /// Implements the abstract DB Listener and listens on the BannerChange channel.
    /// Actions that may come are DELETE, UPDATE, and INSERT.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="services">The set of application services available.</param>
    /// <param name="configuration">The configuration to use.</param>
    [ExcludeFromCodeCoverage]
    public class BannerListener(ILogger<BannerListener> logger, IServiceProvider services, IConfiguration configuration) : BackgroundService
    {
        private const string Channel = "BannerChange";
        private const string ExponentialBaseKey = "ExponentialBase";
        private const string ListenerKey = "BannerListener";
        private const string MaxBackoffDelayKey = "MaxBackoffDelay";
        private const string MaxRetryCountBeforeResetKey = "MaxRetryCountBeforeReset";
        private const string RetryDelayKey = "RetryDelay";

        private static readonly JsonSerializerOptions EventSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new DateTimeConverter(), new JsonStringEnumConverter() },
        };

        private readonly int exponentialBase = configuration.GetValue($"{ListenerKey}:{ExponentialBaseKey}", 2);
        private readonly int maxBackoffDelay = configuration.GetValue($"{ListenerKey}:{MaxBackoffDelayKey}", 32000); // Set the default max backoff delay to 32 seconds
        private readonly int maxRetryCountBeforeReset = configuration.GetValue($"{ListenerKey}:{MaxRetryCountBeforeResetKey}", 5); // Set the default max retry count before reset to 5 attempts
        private readonly int retryDelay = configuration.GetValue($"{ListenerKey}:{RetryDelayKey}", 1000); // Used with exponentialBase and maxBackoffDelay to determine backoffDelay

        /// <summary>
        /// Creates a new DB Connection for push notifications from the DB for a specific channel.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>The task.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Banner Listener is starting");
            stoppingToken.Register(() => logger.LogInformation("Banner Listener Shutdown as cancellation requested"));

            this.ClearCache();
            NpgsqlConnection? con = null;
            int attempts = 0;
            int retryCount = 0; // to determine backoffDelay

            while (!stoppingToken.IsCancellationRequested)
            {
                attempts++;
                logger.LogInformation("Registering Channel Notification on channel {Channel}, attempts = {Attempts}", Channel, attempts);

                try
                {
                    if (con is not { State: ConnectionState.Open })
                    {
                        con?.Dispose(); // Dispose the old connection if it exists
                        con = new NpgsqlConnection(configuration.GetConnectionString("GatewayConnection"));

                        await con.OpenAsync(stoppingToken);

                        // Subscribe to the event
                        con.Notification += this.ReceiveEvent;

                        await using NpgsqlCommand cmd = new();
                        cmd.CommandText = $"""LISTEN "{Channel}";""";
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;

                        await cmd.ExecuteNonQueryAsync(stoppingToken);
                    }

                    // Reset retry count on successful connection
                    retryCount = 0;

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        // Ensure the connection is still open before waiting for events
                        if (con.State != ConnectionState.Open)
                        {
                            throw new NpgsqlException("Database connection closed unexpectedly.");
                        }

                        // Wait for the event
                        await con.WaitAsync(stoppingToken);
                    }
                }
                catch (NpgsqlException e)
                {
                    logger.LogError(e, "Database Error encountered in Banner Listener: {Channel}\n{Message}", Channel, e.Message);
                    retryCount = await this.HandleExceptionAsync(con, retryCount, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unexpected error in Banner Listener");
                    retryCount = await this.HandleExceptionAsync(con, retryCount, stoppingToken);
                }
            }

            con?.Dispose(); // Dispose of the connection when done
            logger.LogWarning("Banner Listener on {Channel} exiting...", Channel);
        }

        private void ClearCache()
        {
            using IServiceScope scope = services.CreateScope();
            ICommunicationService cs = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
            logger.LogInformation("Clearing Banner and InApp Cache");
            cs.ClearCache();
        }

        private async Task<int> HandleExceptionAsync(NpgsqlConnection con, int retryCount, CancellationToken stoppingToken)
        {
            // Unsubscribe the event
            con.Notification -= this.ReceiveEvent;

            retryCount++;

            // Exponential backoff with a cap where exponentialBase is 2, maxBackoffDelay is 32000ms, retryDelay is 1000ms and maxRetryCountBeforeReset is 5.
            // Retry 1: 2 seconds
            // Retry 2: 4 seconds
            // Retry 3: 8 seconds
            // Retry 4: 16 seconds
            // Retry 5: 32 seconds
            // Retry 6: 32 seconds (maxRetryCountBeforeReset at 6 and maxRetryDelay at 32 seconds if applied)
            int backoffDelay = Math.Min(this.retryDelay * (int)Math.Pow(this.exponentialBase, retryCount), this.maxBackoffDelay);
            logger.LogWarning("Retrying due to error in {BackoffDelay}ms (Retry {RetryCount})", backoffDelay, retryCount);

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(backoffDelay, stoppingToken);
            }

            // Reset retry count after a certain number of retries
            if (retryCount >= this.maxRetryCountBeforeReset)
            {
                retryCount = 0;
                logger.LogInformation("Retry count reset after {MaxRetryCountBeforeReset} attempts", this.maxRetryCountBeforeReset);
            }

            return retryCount;
        }

        private void ReceiveEvent(object sender, NpgsqlNotificationEventArgs e)
        {
            logger.LogDebug("Banner Event received on channel {Channel}", Channel);
            BannerChangeEvent? changeEvent = JsonSerializer.Deserialize<BannerChangeEvent>(e.Payload, EventSerializerOptions);
            using IServiceScope scope = services.CreateScope();
            ICommunicationService cs = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
            logger.LogInformation("Banner Event received and being processed");
            cs.ProcessChange(changeEvent);
        }
    }
}
