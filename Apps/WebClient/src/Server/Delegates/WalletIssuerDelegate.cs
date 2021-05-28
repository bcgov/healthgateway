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
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models.AcaPy;
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
        public async Task<RequestResult<ConnectionResponse>> CreateConnectionAsync(Guid walletConnectionId)
        {
            RequestResult<ConnectionResponse> retVal = new RequestResult<ConnectionResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

            this.logger.LogInformation("Create connection invitation");

            List<KeyValuePair<string?, string?>> values = new ();
            FormUrlEncodedContent httpContent = new (values);

            HttpResponseMessage? response = null;
            try
            {
                Uri endpoint = new Uri($"{this.walletIssuerConfig.AgentApiUrl}connections/create-invitation?alias={walletConnectionId}");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    ConnectionResponse? createConnectionResponse = JsonSerializer.Deserialize<ConnectionResponse>(payload);
                    if (createConnectionResponse != null)
                    {
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = createConnectionResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Create connection invitation response parse error {payload}");
                        retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception Creating Connection: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in CreateConnectionAsync {e}");
            }
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }

        /// <inheritdoc/>
        public Task<RequestResult<CredentialResponse>> CreateCredentialAsync<T>(WalletConnection connection, T payload)
            where T : CredentialPayload
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<RequestResult<WalletConnection>> DisconnectConnectionAsync(WalletConnection connection)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<RequestResult<WalletCredential>> RevokeCredentialAsync(WalletCredential credential)
        {
            throw new NotImplementedException();
        }

        private HttpClient InitializeClient()
        {
            string bearerToken = this.walletIssuerConfig.AgentApiKey;

            HttpClient httpClient = this.httpClientService.CreateDefaultHttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
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
