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
namespace HealthGateway.WebClient.Listeners
{
    using System;
    using System.Data;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    /// <summary>
    /// Implements the abstract DB Listener and listens on the BannerChange channel.
    /// </summary>
    public class BannerListener : BackgroundService
    {
        private const string DeleteAction = "DELETE";
        private const string UpdateAction = "UPDATE";
        private const string InsertAction = "INSERT";
        private const string Channel = "BannerChange";

        private readonly ILogger<BannerListener> logger;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="BannerListener"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="services">The set of application services available.</param>
        public BannerListener(
            ILogger<BannerListener> logger,
            IConfiguration configuration,
            IServiceProvider services)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.services = services;
        }

        private ICommunicationService? CommunicationService { get; set; }

        /// <summary>
        /// Creates a new DB Connection for push notifications from the DB for a specific channel.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to use.</param>
        /// <returns>The task.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Abstract class property")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogDebug($"Registering Channel Notification on channel {Channel}");
            stoppingToken.Register(() =>
                this.logger.LogInformation($"DBChangeListener exiting..."));
            try
            {
                using var scope = this.services.CreateScope();
                GatewayDbContext dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
                this.CommunicationService = scope.ServiceProvider.GetRequiredService<ICommunicationService>();
                NpgsqlConnection con = (NpgsqlConnection)dbContext.Database.GetDbConnection();
                con.Open();
                con.Notification += this.ReceiveEvent;
                using NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.CommandText = @$"LISTEN ""{Channel}"";";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = con;
                cmd.ExecuteNonQuery();

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Wait for the event
                    await con.WaitAsync(stoppingToken).ConfigureAwait(true);
                }

                this.logger.LogInformation($"Exiting WaitChannelNotification: {Channel}");
            }
            catch (NpgsqlException e)
            {
                this.logger.LogError($"DB Error encountered in WaitChannelNotification: {Channel}\n{e.ToString()}");
            }
        }

        private void ReceiveEvent(object sender, NpgsqlNotificationEventArgs e)
        {
            this.logger.LogDebug($"Event received on channel {Channel}");
            BannerChangeEvent changeEvent = JsonSerializer.Deserialize<BannerChangeEvent>(e.Payload);
            if (this.CommunicationService != null && changeEvent.Data != null)
            {
                DateTime utcnow = DateTime.UtcNow;
                RequestResult<Communication> cacheEntry = new RequestResult<Communication>();
                if (changeEvent.Action == InsertAction ||
                    changeEvent.Action == UpdateAction)
                {
                    Communication comm = changeEvent.Data;
                    if (utcnow >= comm.EffectiveDateTime && utcnow <= comm.ExpiryDateTime)
                    {
                        cacheEntry.ResultStatus = Common.Constants.ResultType.Success;
                        cacheEntry.ResourcePayload = comm;
                        this.logger.LogInformation("Active Banner inserted or updated in DB");
                        this.CommunicationService.SetActiveBannerCache(cacheEntry);
                    }
                }
                else if (changeEvent.Action == DeleteAction)
                {
                    RequestResult<Communication> currentBanner = this.CommunicationService.GetActiveBanner();
                    if (currentBanner.ResourcePayload != null &&
                        currentBanner.ResourcePayload.Id == changeEvent.Data.Id)
                    {
                        cacheEntry.ResultStatus = Common.Constants.ResultType.Error;
                        cacheEntry.ResultError = new RequestResultError() { ResultMessage = "Active Banner deleted from DB", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                        this.logger.LogInformation("Active Banner deleted from DB");
                        this.CommunicationService.SetActiveBannerCache(cacheEntry);
                    }
                }
            }
        }
    }
}
