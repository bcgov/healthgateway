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
namespace HealthGateway.WebClient.Controllers
{
    using System;
    using System.Text.Json;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Filters;
    using HealthGateway.WebClient.Models.AcaPy;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides a web hook for Aca-Py to receive status updates on Wallet Connections and Credentials.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [IgnoreAudit]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class WalletStatusController : Controller
    {
        private readonly ILogger logger;
        private readonly IWalletStatusService walletStatusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletStatusController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="walletStatusService">The injected wallet status service provider.</param>
        public WalletStatusController(ILogger<WalletStatusController> logger, IWalletStatusService walletStatusService)
        {
            this.logger = logger;
            this.walletStatusService = walletStatusService;
        }

        /// <summary>
        /// Handle webhook events sent from the issuing agent.
        /// </summary>
        /// <param name="topic">The type of webhook response (connection or issue credential).</param>
        /// <param name="data">Webhook response data.</param>
        /// <returns>An empty response.</returns>
        /// <response code="204">Webhook request received.</response>
        [HttpPost]
        [Route("topic/{topic}")]
        [Authorize(Policy = ApiKeyPolicy.Write)]
        public IActionResult Webhook(string topic, [FromBody] WebhookData data)
        {
            this.logger.LogInformation("Webhook topic \"{topic}\"", topic);
            switch (topic)
            {
                case WebhookTopic.Connections:
                    this.HandleConnectionUpdateAsync(data);
                    break;
                case WebhookTopic.IssueCredential:
                    this.HandleIssueCredentialUpdateAsync(data);
                    break;
                case WebhookTopic.RevocationRegistry:
                    break;
                case WebhookTopic.BasicMessage:
                    this.logger.LogInformation("Basic Message data: for {@JObject}", JsonSerializer.Serialize(data));
                    break;
                default:
                    this.logger.LogInformation("Webhook {topic} is not supported", topic);
                    break;
            }

            return this.NoContent();
        }

        // Handle webhook events for connection states.
        private void HandleConnectionUpdateAsync(WebhookData data)
        {
            this.logger.LogInformation("Connection state \"{state}\" for {@JObject}", data.State, JsonSerializer.Serialize(data));

            switch (data.State)
            {
                case ConnectionState.Invitation:
                    break;
                case ConnectionState.Request:
                    break;
                case ConnectionState.Response:
                    this.walletStatusService.UpdateWalletConnection(new Guid(data.Alias));
                    break;
                case ConnectionState.Active:
                    break;
                default:
                    this.logger.LogError("Connection state {state} is not supported", data.State);
                    break;
            }
        }

        // Handle webhook events for issue credential topics.
        private void HandleIssueCredentialUpdateAsync(WebhookData data)
        {
            this.logger.LogInformation("Issue credential state \"{state}\" for {@JObject}", data.State, JsonSerializer.Serialize(data));

            switch (data.State)
            {
                case CredentialExchangeState.OfferSent:
                    break;
                case CredentialExchangeState.RequestReceived:
                    break;
                case CredentialExchangeState.CredentialIssued:
                    this.walletStatusService.UpdateWalletCredential(data.CredentialExchangeId);
                    break;
                default:
                    this.logger.LogError("Credential exchange state {state} is not supported", data.State);
                    break;
            }
        }
    }
}
