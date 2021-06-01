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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
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
            this.WalletIssuerConfig = new WalletIssuerConfiguration();
            configuration.Bind(AcapyConfigSectionKey, this.WalletIssuerConfig);

            this.client = this.InitializeClient();
        }

        /// <inheritdoc/>
        public WalletIssuerConfiguration WalletIssuerConfig { get; private set; }

        /// <inheritdoc/>
        public async Task<RequestResult<ConnectionResponse>> CreateConnectionAsync(Guid walletConnectionId)
        {
            RequestResult<ConnectionResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogInformation("Create connection invitation");

            List<KeyValuePair<string?, string?>> values = new ();
            FormUrlEncodedContent httpContent = new (values);

            HttpResponseMessage? response;
            try
            {
                Uri endpoint = new Uri($"{this.WalletIssuerConfig.AgentApiUrl}connections/create-invitation?alias={walletConnectionId}");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    ConnectionResponse? createConnectionResponse = JsonSerializer.Deserialize<ConnectionResponse>(payload);
                    if (createConnectionResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
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
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CredentialResponse>> CreateCredentialAsync<T>(WalletConnection connection, T payload, string comment)
            where T : CredentialPayload
        {
            _ = connection.AgentId ?? throw new ArgumentException("AgendId of Connection cannot be null");
            RequestResult<CredentialResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            RequestResult<IssuerDidResponse> issuerDidResponse = await this.GetIssuerDidAsync().ConfigureAwait(true);
            if (issuerDidResponse.ResultStatus == ResultType.Success)
            {
                string did = issuerDidResponse.ResourcePayload!.Result.Did;

                RequestResult<string> schemaIdResponse = await this.GetSchemaIdAsync(did).ConfigureAwait(true);
                if (schemaIdResponse.ResultStatus == ResultType.Success)
                {
                    string schemaId = schemaIdResponse.ResourcePayload!;

                    RequestResult<CredentialDefinitionIdResponse> credentialDefinitionIdResponse = await this.GetCredentialDefinitionIdsAsync(schemaId).ConfigureAwait(true);
                    if (credentialDefinitionIdResponse.ResultStatus == ResultType.Success)
                    {
                        string credentialDefinitionId = credentialDefinitionIdResponse.ResourcePayload!.CredentialDefinitionIds.First();
                        CredentialProposal credentialProposal = new ();
                        foreach (PropertyInfo property in payload.GetType().GetProperties())
                        {
                            if (property != null)
                            {
                                credentialProposal.Attributes.Add(new CredentialAttribute
                                {
                                    Name = property.Name,
                                    Value = property.GetValue(payload)?.ToString(),
                                });
                            }
                        }

                        CredentialOfferRequest credentialOffer = new CredentialOfferRequest
                        {
                            ConnectionId = connection.AgentId,
                            IssuerDid = did,
                            SchemaId = schemaId,
                            SchemaIssuerDid = did,
                            SchemaName = this.WalletIssuerConfig.SchemaName,
                            SchemaVersion = this.WalletIssuerConfig.SchemaVersion,
                            CredentialDefinitionId = credentialDefinitionId,
                            Comment = comment,
                            CredentialProposal = credentialProposal,
                        };

                        RequestResult<CredentialResponse> credentialResponse = await this.IssueCredentialAsync(credentialOffer).ConfigureAwait(true);
                        if (credentialResponse.ResultStatus == ResultType.Success)
                        {
                            retVal.ResourcePayload = credentialResponse.ResourcePayload;
                            retVal.TotalResultCount = 1;
                            retVal.ResultStatus = ResultType.Success;
                        }
                        else
                        {
                            retVal.ResultError = credentialResponse.ResultError;
                        }
                    }
                    else
                    {
                        retVal.ResultError = credentialDefinitionIdResponse.ResultError;
                    }
                }
                else
                {
                    retVal.ResultError = schemaIdResponse.ResultError;
                }
            }
            else
            {
                retVal.ResultError = issuerDidResponse.ResultError;
            }

            return retVal;
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

        /// <inheritdoc/>
        public async Task<RequestResult<SchemaResponse>> CreateSchemaAsync(SchemaRequest schema)
        {
            RequestResult<SchemaResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Creating Schema");

            StringContent httpContent = new (JsonSerializer.Serialize(schema), Encoding.UTF8, "application/json");

            HttpResponseMessage? response;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}schemas");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    SchemaResponse? schemaResponse = JsonSerializer.Deserialize<SchemaResponse>(payload);
                    if (schemaResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = schemaResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Create schema response parse error {payload}");
                        retVal.ResultError = new () { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new () { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new () { ResultMessage = $"Exception creating schema: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in CreateSchemaAsync {e}");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CredentialDefinitionResponse>> CreateCredentialDefinitionAsync(string schemaId)
        {
            RequestResult<CredentialDefinitionResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogInformation("Create credential definition.");

            CredentialDefinitionRequest credentialDefinition = new ()
            {
                SchemaId = schemaId,
                SupportRevocation = true,
                Tag = "health_gateway_bc",
            };

            StringContent httpContent = new (JsonSerializer.Serialize(credentialDefinition), Encoding.UTF8, "application/json");

            HttpResponseMessage? response;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}credential-definitions");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    CredentialDefinitionResponse? definitionResponse = JsonSerializer.Deserialize<CredentialDefinitionResponse>(payload);
                    if (definitionResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
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
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }

        private HttpClient InitializeClient()
        {
            string apiKey = this.WalletIssuerConfig.AgentApiKey;

            HttpClient httpClient = this.httpClientService.CreateDefaultHttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            return httpClient;
        }

        private async Task<RequestResult<IssuerDidResponse>> GetIssuerDidAsync()
        {
            RequestResult<IssuerDidResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Getting issuer did");

            HttpResponseMessage? response;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}wallet/did/public");
                response = await this.client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    IssuerDidResponse? didResponse = JsonSerializer.Deserialize<IssuerDidResponse>(payload);
                    if (didResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = didResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Get issuer did response parse error {payload}");
                        retVal.ResultError = new () { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new () { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new () { ResultMessage = $"Exception getting issuer did: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in GetIssuerDidAsync {e}");
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return retVal;
        }

        private async Task<RequestResult<string>> GetSchemaIdAsync(string did)
        {
            RequestResult<string> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Getting schema id");

            HttpResponseMessage? response = null;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}schemas/created?schema_version={this.WalletIssuerConfig.SchemaVersion}&schema_issuer_did={did}&schema_name={this.WalletIssuerConfig.SchemaName}");
                response = await this.client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    SchemaIdResponse? schemaIdResponse = JsonSerializer.Deserialize<SchemaIdResponse>(payload);
                    if (schemaIdResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = schemaIdResponse.SchemaIds.First();
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Get schema id response parse error {payload}");
                        retVal.ResultError = new () { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new () { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new () { ResultMessage = $"Exception getting schema id: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in GetSchemaIdAsync {e}");
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return retVal;
        }

        private async Task<RequestResult<CredentialDefinitionIdResponse>> GetCredentialDefinitionIdsAsync(string schemaId)
        {
            RequestResult<CredentialDefinitionIdResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Getting credential definition id");

            HttpResponseMessage? response;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}credential-definitions/created?schema_id={schemaId}");
                response = await this.client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    CredentialDefinitionIdResponse? definitionIdResponse = JsonSerializer.Deserialize<CredentialDefinitionIdResponse>(payload);
                    if (definitionIdResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = definitionIdResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Get credential definition id response parse error {payload}");
                        retVal.ResultError = new () { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new () { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new () { ResultMessage = $"Exception getting credential definition id: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in GetCredentialDefinitionIdAsync {e}");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return retVal;
        }

        private async Task<RequestResult<CredentialResponse>> IssueCredentialAsync(CredentialOfferRequest credentialOffer)
        {
            RequestResult<CredentialResponse> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Issuing Credential");

            StringContent httpContent = new (JsonSerializer.Serialize(credentialOffer), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                Uri endpoint = new ($"{this.WalletIssuerConfig.AgentApiUrl}issue-credential/send");
                response = await this.client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                if (response.IsSuccessStatusCode)
                {
                    CredentialResponse? credentialResponse = JsonSerializer.Deserialize<CredentialResponse>(payload);
                    if (credentialResponse != null)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = credentialResponse;
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Create credential response parse error {payload}");
                        retVal.ResultError = new () { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    retVal.ResultError = new () { ResultMessage = $"Unable to connect to AcaPy Agent, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                retVal.ResultError = new () { ResultMessage = $"Exception issuing credential: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                this.logger.LogError($"Unexpected exception in IssueCredentialAsync {e}");
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                httpContent.Dispose();
            }

            return retVal;
        }
    }
}
