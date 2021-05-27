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
namespace HealthGateway.WebClient.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Server.Models.AcaPy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to create/revoke Connections and Credentials.
    /// </summary>
    public class WalletIssuerDelegate : IWalletIssuerDelegate
    {
        private const string AcapyConfigSectionKey = "AcaPy";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly HttpClient client;
        private readonly WalletIssuerConfiguration walletIssuerConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletIssuerDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public WalletIssuerDelegate(
            ILogger<WalletIssuerDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.walletIssuerConfig = new WalletIssuerConfiguration();
            configuration.Bind(AcapyConfigSectionKey, this.walletIssuerConfig);

            this.client = this.InitializeClient();
        }

        /// <inheritdoc/>
        public async Task<CreateConnectionResponse> CreateConnectionAsync(string walletConnectionId)
        {
            this.logger.LogInformation("Create connection invitation");

            List<KeyValuePair<string?, string?>> values = new ();
            FormUrlEncodedContent httpContent = new (values);

            HttpResponseMessage? response = null;
            try
            {
                response = await this.client.PostAsync(new Uri($"connections/create-invitation?alias={walletConnectionId}"), httpContent).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await this.LogError(httpContent, response, ex).ConfigureAwait(true);
                throw new HttpRequestException("Error occurred when calling AcaPy API. Try again later.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await this.LogError(httpContent, response).ConfigureAwait(true);
                throw new HttpRequestException($"Error code {response.StatusCode} was provided when calling WalletIssuerDelegate::CreateInvitationAsync");
            }

            httpContent.Dispose();

            CreateConnectionResponse createConnectionResponse = await response.Content.ReadAsAsync<CreateConnectionResponse>().ConfigureAwait(true);

            this.logger.LogInformation("Create connection invitation response {@JObject}", JsonSerializer.Serialize(createConnectionResponse));

            return createConnectionResponse;
        }

        private HttpClient InitializeClient()
        {
            string bearerToken = this.walletIssuerConfig.AgentApiKey;

            HttpClient httpClient = this.httpClientService.CreateDefaultHttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            httpClient.BaseAddress = this.walletIssuerConfig.AgentApiUrl;

            return httpClient;
        }

        private async Task LogError(HttpContent content, HttpResponseMessage response, Exception? exception = null)
        {
            string secondaryMessage;
            if (exception != null)
            {
                secondaryMessage = $"Exception: {exception.Message}";
                this.logger.LogError(exception, secondaryMessage, new object[] { content, response });
            }
            else if (response != null)
            {
                string responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                secondaryMessage = $"Response code: {(int)response.StatusCode}, response body:{responseMessage}";
                this.logger.LogError(exception, secondaryMessage, new object[] { content, response });
            }
            else
            {
                secondaryMessage = "No additional message. Http response and exception were null.";
                this.logger.LogError(exception, secondaryMessage, new object[] { content });
            }
        }
    }
}
