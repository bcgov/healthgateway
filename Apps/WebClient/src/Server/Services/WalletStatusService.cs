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
namespace HealthGateway.WebClient.Services
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models.AcaPy;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class WalletStatusService : IWalletStatusService
    {
        private readonly ILogger logger;
        private readonly IWalletDelegate walletDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletStatusService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="walletDelegate">Injected Wallet delegate.</param>
        public WalletStatusService(ILogger<WalletStatusService> logger, IWalletDelegate walletDelegate)
        {
            this.logger = logger;
            this.walletDelegate = walletDelegate;
        }

        /// <inheritdoc />
        public bool WebhookAsync(string topic, WebhookData data)
        {
            this.logger.LogInformation("Webhook topic \"{topic}\"", topic);

            switch (topic)
            {
                case WebhookTopic.Connections:
                    return this.HandleConnectionAsync(data);
                case WebhookTopic.IssueCredential:
                    return this.HandleIssueCredentialAsync(data);
                case WebhookTopic.RevocationRegistry:
                    return true;
                case WebhookTopic.BasicMessage:
                    this.logger.LogInformation("Basic Message data: for {@JObject}", JsonSerializer.Serialize(data));
                    return false;
                default:
                    this.logger.LogError("Webhook {topic} is not supported", topic);
                    return false;
            }
        }

        /// <inheritdoc />
        public void UpdateWalletConnection(Guid connectionId)
        {
            DBResult<WalletConnection> dbResult = this.walletDelegate.GetConnection(connectionId);
            WalletConnection connection = dbResult.Payload;
            connection.ConnectedDateTime = DateTime.Now;
            connection.Status = WalletConnectionStatus.Connected;
            this.walletDelegate.UpdateConnection(connection, true);
        }

        /// <inheritdoc />
        public void UpdateWalletCredential(Guid exchangeId)
        {
            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredential(exchangeId);
            WalletCredential credential = dbResult.Payload;
            credential.AddedDateTime = DateTime.Now;
            credential.Status = WalletCredentialStatus.Added;
            this.walletDelegate.UpdateCredential(credential, true);
        }

        // Handle webhook events for connection states.
        private bool HandleConnectionAsync(WebhookData data)
        {
            this.logger.LogInformation("Connection state \"{state}\" for {@JObject}", data.State, JsonSerializer.Serialize(data));

            switch (data.State)
            {
                case ConnectionState.Invitation:
                    return true;

                case ConnectionState.Request:
                    return true;

                case ConnectionState.Response:
                    this.UpdateWalletConnection(new Guid(data.Alias));
                    return true;

                case ConnectionState.Active:
                    return true;

                default:
                    this.logger.LogError("Connection state {state} is not supported", data.State);
                    return false;
            }
        }

        // Handle webhook events for issue credential topics.
        private bool HandleIssueCredentialAsync(WebhookData data)
        {
            this.logger.LogInformation("Issue credential state \"{state}\" for {@JObject}", data.State, JsonSerializer.Serialize(data));

            switch (data.State)
            {
                case CredentialExchangeState.OfferSent:
                    return true;
                case CredentialExchangeState.RequestReceived:
                    return true;
                case CredentialExchangeState.CredentialIssued:
                    this.UpdateWalletCredential(data.CredentialExchangeId);
                    return true;
                default:
                    this.logger.LogError("Credential exchange state {state} is not supported", data.State);
                    return false;
            }
        }
    }
}
