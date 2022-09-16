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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <summary>
    /// Implements the abstract DB Listener and listens on the BannerChange channel.
    /// Actions that may come are DELETE, UPDATE, and INSERT.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BannerListener : BackgroundService
    {
        private const string Channel = "BannerChange";
        private const int SleepDuration = 10000;

        private readonly ILogger<BannerListener> logger;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="BannerListener"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="services">The set of application services available.</param>
        public BannerListener(
            ILogger<BannerListener> logger,
            IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;
        }

        /// <summary>
        /// Creates a new DB Connection for push notifications from the DB for a specific channel.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>The task.</returns>
        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Abstract class property")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Banner Listener is starting");
            stoppingToken.Register(() => this.logger.LogInformation("Banner Listener Shutdown as cancellation requested    "));
            this.ClearCache();
            int attempts = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                attempts++;
                this.logger.LogInformation("Registering Channel Notification on channel {Channel}, attempts = {Attempts}", Channel, attempts);
                try
                {
                    using IServiceScope scope = this.services.CreateScope();
                    using GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
                    using NpgsqlConnection con = (NpgsqlConnection)dbContext.Database.GetDbConnection();
                    await con.OpenAsync(stoppingToken).ConfigureAwait(true);
                    con.Notification += this.ReceiveEvent;
                    using NpgsqlCommand cmd = new();
                    cmd.CommandText = @$"LISTEN ""{Channel}"";";
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = con;
                    await cmd.ExecuteNonQueryAsync(stoppingToken).ConfigureAwait(true);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        // Wait for the event
                        await con.WaitAsync(stoppingToken).ConfigureAwait(true);
                    }

                    await con.CloseAsync().ConfigureAwait(true);
                }
                catch (NpgsqlException e)
                {
                    this.logger.LogError("DB Error encountered in WaitChannelNotification: {Channel}\n{Exception}", Channel, e.ToString());
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(SleepDuration, stoppingToken).ConfigureAwait(true);
                    }
                }

                this.logger.LogWarning($"Banner Listener on {Channel} exiting...");
            }
        }

        private void ClearCache()
        {
            using IServiceScope scope = this.services.CreateScope();
            ICommunicationService cs = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
            this.logger.LogInformation("Clearing Banner and InApp Cache");
            cs.ClearCache();
        }

        private void ReceiveEvent(object sender, NpgsqlNotificationEventArgs e)
        {
            this.logger.LogDebug($"Banner Event received on channel {Channel}");
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new DateTimeConverter());
            options.Converters.Add(new JsonStringEnumConverter());
            BannerChangeEvent? changeEvent = JsonSerializer.Deserialize<BannerChangeEvent>(e.Payload, options);
            using IServiceScope scope = this.services.CreateScope();
            ICommunicationService cs = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
            this.logger.LogInformation("Banner Event received and being processed");
            cs.ProcessChange(changeEvent);
        }
    }
}
