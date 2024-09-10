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
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore;
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
        private const string ListenerKey = "BannerListener";
        private const string MaxRetryAttemptsKey = "MaxRetryAttempts";
        private const string SleepDurationKey = "SleepDuration";

        private static readonly JsonSerializerOptions EventSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new DateTimeConverter(), new JsonStringEnumConverter() },
        };

        private readonly int maxRetryAttempts = configuration.GetValue($"{ListenerKey}:{MaxRetryAttemptsKey}", 5);
        private readonly int sleepDuration = configuration.GetValue($"{ListenerKey}:{SleepDurationKey}", 10000);

        /// <summary>
        /// Creates a new DB Connection for push notifications from the DB for a specific channel.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>The task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Banner Listener is starting");
            stoppingToken.Register(() => logger.LogInformation("Banner Listener Shutdown as cancellation requested"));
            this.ClearCache();
            int attempts = 0;
            int retryCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                attempts++;
                logger.LogInformation("Registering Channel Notification on channel {Channel}, attempts = {Attempts}", Channel, attempts);
                try
                {
                    using IServiceScope scope = services.CreateScope();
                    await using GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
                    await using NpgsqlConnection con = (NpgsqlConnection)dbContext.Database.GetDbConnection();
                    await con.OpenAsync(stoppingToken);
                    con.Notification += this.ReceiveEvent;

                    await using NpgsqlCommand cmd = new();
                    cmd.CommandText = $"""LISTEN "{Channel}";""";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    await cmd.ExecuteNonQueryAsync(stoppingToken);

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

                    await con.CloseAsync();
                }
                catch (NpgsqlException e)
                {
                    retryCount++;
                    logger.LogError(e, "DB Error encountered in WaitChannelNotification: {Channel}\n{Message}", Channel, e.Message);

                    if (retryCount <= this.maxRetryAttempts)
                    {
                        // Exponential backoff with max retry attempts
                        int backoffDelay = this.sleepDuration * (int)Math.Pow(2, retryCount);
                        logger.LogWarning("Retrying connection in {BackoffDelay}ms (Retry {RetryCount}/{MaxRetryAttempts})", backoffDelay, retryCount, this.maxRetryAttempts);

                        if (!stoppingToken.IsCancellationRequested)
                        {
                            await Task.Delay(backoffDelay, stoppingToken);
                        }
                    }
                    else
                    {
                        logger.LogError("Max retry attempts reached. Stopping Banner Listener for {Channel}", Channel);
                        break;
                    }
                }

                logger.LogWarning("Banner Listener on {Channel} exiting...", Channel);
            }
        }

        private void ClearCache()
        {
            using IServiceScope scope = services.CreateScope();
            ICommunicationService cs = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
            logger.LogInformation("Clearing Banner and InApp Cache");
            cs.ClearCache();
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
