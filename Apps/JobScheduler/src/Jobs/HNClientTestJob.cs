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
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Context;
    using Healthgateway.JobScheduler.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Validates that the HNClient Endpoint is responding correctly.
    /// If the endpoint does not respond then an email will be sent.
    /// </summary>
    public class HNClientTestJob
    {
        private const string JobKey = "HNClientTest";
        private const string RocketChatKey = "RocketChat";
        private const string HNClientProxyKey = "HNClientProxyURL";
        private const int ConcurrencyTimeout = 5 * 60; // 5 Minutes

        private readonly Uri hnclientProxyURL;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;
        private readonly IHttpClientService httpClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HNClientTestJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dbContext">The db context to use.</param>
        /// <param name="httpClientService">The http client to use.</param>"
        public HNClientTestJob(
            IConfiguration configuration,
            ILogger<HNClientTestJob> logger,
            GatewayDbContext dbContext,
            IHttpClientService httpClientService)
        {
            this.configuration = configuration;
            this.hnclientProxyURL = configuration.GetValue<Uri>($"{JobKey}:{HNClientProxyKey}");
            this.logger = logger;
            this.dbContext = dbContext;
            this.httpClientService = httpClientService;
        }

        /// <summary>
        /// Runs the verification and queues the email.
        /// </summary>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void Process()
        {
            this.CheckHNClient().Wait();
        }

        /// <summary>
        /// Runs the verification and sends an alert to Rocket Chat if required.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task CheckHNClient()
        {
            // Check if HNClient is running normally
            bool hncError = false;
            this.logger.LogTrace($"Starting to verify HNClient {ConcurrencyTimeout}");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            HttpResponseMessage response = await client.GetAsync(this.hnclientProxyURL).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                HNClientResult result = JsonSerializer.Deserialize<HNClientResult>(json);
                hncError = result.Error;
            }
            else
            {
                hncError = false;
            }

            if (hncError)
            {
                // If HNClient is in an error state, post a message to Rocket Chat to indicate HNClient is down.
                RocketChatConfig rcCfg = this.configuration.GetSection($"{JobKey}:{RocketChatKey}").Get<RocketChatConfig>();
                string json = JsonSerializer.Serialize(rcCfg.Message);
                this.logger.LogInformation($"HNClient is down, sending message to RocketChat {json}");
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, rcCfg.WebHookURL)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };

                _ = await client.SendAsync(request).ConfigureAwait(true);
            }

            this.logger.LogDebug($"Finished verifying HNClient {ConcurrencyTimeout}");
        }
    }
}
