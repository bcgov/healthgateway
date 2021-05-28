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
namespace Healthgateway.JobScheduler.Tasks
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.WebClient.Models.AcaPy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Queries the Health Gateway DB for all current users with emails.
    /// Queues NotificationSettings job for each.
    /// </summary>
    public class CreateImmunizationSchema : IOneTimeTask
    {
        private const string AcapyConfigSectionKey = "AcaPy";
        private readonly ILogger<CreateImmunizationSchema> logger;
        private readonly WalletIssuerConfiguration walletIssuerConfig;
        private readonly IHttpClientService httpClientService;
        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateImmunizationSchema"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public CreateImmunizationSchema(
                ILogger<CreateImmunizationSchema> logger,
                IHttpClientService httpClientService,
                IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.walletIssuerConfig = new WalletIssuerConfiguration();
            configuration.Bind(AcapyConfigSectionKey, this.walletIssuerConfig);

            this.client = this.InitializeClient();
        }

        /// <summary>
        /// Runs the task that needs to be done for the IOneTimeTask.
        /// </summary>
        public void Run()
        {
            this.logger.LogInformation($"Performing Task {this.GetType().Name}");
            RequestResult<CreateSchemaResponse> schemaResponse = Task.Run(async () => await this.CreateSchemaAsync().ConfigureAwait(true)).Result;
            if (schemaResponse.ResourcePayload != null)
            {
                Task.Run(async () => await this.CreateCredentialDefinitionAsync(schemaResponse.ResourcePayload.SchemaId).ConfigureAwait(true));
            }

            this.logger.LogInformation($"Task {this.GetType().Name} has completed");
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

        private async Task<RequestResult<CreateSchemaResponse>> CreateSchemaAsync()
        {
            RequestResult<CreateSchemaResponse> retVal = new RequestResult<CreateSchemaResponse>()
            {
                ResultStatus = HealthGateway.Common.Constants.ResultType.Error,
            };

            this.logger.LogInformation("Create Schema");

            var schema = new CreateSchemaRequest
            {
                SchemaName = this.walletIssuerConfig.SchemaName,
                SchemaVersion = this.walletIssuerConfig.SchemaVersion,
            };

            foreach (var property in typeof(ImmunizationCredential).GetProperties())
            {
                schema.Attributes?.Add(property.Name);
            }

            var httpContent = new StringContent(schema.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                Uri endpoint = new Uri($"{this.walletIssuerConfig.AgentApiUrl}schemas");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    CreateSchemaResponse? schemaResponse = JsonSerializer.Deserialize<CreateSchemaResponse>(payload);
                    if (schemaResponse != null)
                    {
                        retVal.ResultStatus = HealthGateway.Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = schemaResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Create schema response parse error {payload}");
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
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception creating schema: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in CreateSchemaAsync {e}");
            }
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }

        private async Task<RequestResult<CreateCredentialDefinitionResponse>> CreateCredentialDefinitionAsync(string schemaId)
        {
            RequestResult<CreateCredentialDefinitionResponse> retVal = new RequestResult<CreateCredentialDefinitionResponse>()
            {
                ResultStatus = HealthGateway.Common.Constants.ResultType.Error,
            };

            this.logger.LogInformation("Create credential definition.");

            var credentialDefinition = new CreateCredentialDefinitionRequest
            {
                SchemaId = schemaId,
                SupportRevocation = true,
                Tag = "health_gateway_bc",
            };

            var httpContent = new StringContent(credentialDefinition.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                Uri endpoint = new Uri($"{this.walletIssuerConfig.AgentApiUrl}credential-definitions");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    CreateCredentialDefinitionResponse? definitionResponse = JsonSerializer.Deserialize<CreateCredentialDefinitionResponse>(payload);
                    if (definitionResponse != null)
                    {
                        retVal.ResultStatus = HealthGateway.Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = definitionResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Create credential definition response parse error {payload}");
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
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception creating credential definition: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in CreateCredentialDefinitionAsync {e}");
            }
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }
    }
}
