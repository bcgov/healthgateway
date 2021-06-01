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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class WalletService : IWalletService
    {
        private const string ImmunizationResourceType = "Immunization";
        private const string CredentialComment = "Immunization Credential";
        private readonly ILogger logger;
        private readonly IWalletDelegate walletDelegate;
        private readonly IWalletIssuerDelegate walletIssuerDelegate;
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;
        private readonly IImmunizationDelegate immunizationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="walletDelegate">Injected Wallet delegate.</param>
        /// <param name="walletIssuerDelegate">Injected Wallet issuer delegate.</param>
        /// <param name="clientRegistriesDelegate">Injected client registries delegate.</param>
        /// <param name="immunizationDelegate">Injected immunization delegate.</param>
        public WalletService(
            ILogger<WalletService> logger,
            IWalletDelegate walletDelegate,
            IWalletIssuerDelegate walletIssuerDelegate,
            IClientRegistriesDelegate clientRegistriesDelegate,
            IImmunizationDelegate immunizationDelegate)
        {
            this.logger = logger;
            this.walletDelegate = walletDelegate;
            this.walletIssuerDelegate = walletIssuerDelegate;
            this.clientRegistriesDelegate = clientRegistriesDelegate;
            this.immunizationDelegate = immunizationDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<WalletConnectionModel>> CreateConnectionAsync(string hdId, IEnumerable<string> targetIds)
        {
            this.logger.LogDebug($"Creating wallet connection and credentials {JsonSerializer.Serialize(targetIds)} for user {hdId}");
            RequestResult<WalletConnectionModel> walletConnectionResult = await this.CreateConnectionAsync(hdId).ConfigureAwait(true);
            if (walletConnectionResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogDebug($"Error creating wallet connection {JsonSerializer.Serialize(walletConnectionResult)} for user {hdId}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error creating wallet connection",
                        InnerError = walletConnectionResult.ResultError,
                    },
                };
            }

            this.logger.LogDebug($"Creating wallet credentials {JsonSerializer.Serialize(targetIds)} for user {hdId}");
            RequestResult<IEnumerable<WalletCredentialModel>> walletCredentialsResult = await this.CreateCredentialsAsync(hdId, targetIds).ConfigureAwait(true);
            if (walletCredentialsResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogDebug($"Error creating wallet credentials {JsonSerializer.Serialize(walletCredentialsResult)} for user {hdId}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error creating wallet credentials",
                        InnerError = walletConnectionResult.ResultError,
                    },
                };
            }

            walletConnectionResult = this.GetConnection(hdId);

            this.logger.LogDebug($"Finished creating wallet connection and credentials {JsonSerializer.Serialize(targetIds)} for user {hdId}: {JsonSerializer.Serialize(walletConnectionResult)}");
            return walletConnectionResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<WalletConnectionModel>> CreateConnectionAsync(string hdId)
        {
            this.logger.LogDebug($"Inserting wallet connection to database. user {hdId}");
            WalletConnection walletConnection = new WalletConnection();
            walletConnection.UserProfileId = hdId;
            walletConnection.Status = WalletConnectionStatus.Pending;
            DBResult<WalletConnection> dbResult = this.walletDelegate.InsertConnection(walletConnection);
            if (dbResult.Status != DBStatusCode.Created)
            {
                this.logger.LogDebug($"Error inserting wallet connection to database. user {hdId}: {JsonSerializer.Serialize(dbResult)}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error inserting wallet connection to database",
                    },
                };
            }

            this.logger.LogDebug($"Finished Inserting wallet connection to database. user {hdId}: {JsonSerializer.Serialize(dbResult)}");
            this.logger.LogDebug($"Creating connection with wallet issuer. user {hdId}");
            RequestResult<ConnectionResponse> walletIssuerConnectionResult =
                await this.walletIssuerDelegate.CreateConnectionAsync(walletConnection.Id).ConfigureAwait(true);
            if (walletIssuerConnectionResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogDebug($"Error creating connection with wallet issuer. user {hdId}: {JsonSerializer.Serialize(walletIssuerConnectionResult)}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error creating wallet connection with wallet issuer",
                        InnerError = walletIssuerConnectionResult.ResultError,
                    },
                };
            }

            this.logger.LogDebug($"Finished creating connection with wallet issuer. user {hdId}: {JsonSerializer.Serialize(walletIssuerConnectionResult)}");
            this.logger.LogDebug($"Updating wallet connection to database. user {hdId}");
            ConnectionResponse walletIssuerConnection = walletIssuerConnectionResult.ResourcePayload!;
            walletConnection.InvitationEndpoint = walletIssuerConnection.InvitationUrl?.AbsoluteUri;
            walletConnection.AgentId = walletIssuerConnection.AgentId;
            dbResult = this.walletDelegate.UpdateConnection(walletConnection);
            if (dbResult.Status != DBStatusCode.Updated)
            {
                this.logger.LogDebug($"Error updating wallet connection to database. user {hdId}: {JsonSerializer.Serialize(dbResult)}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error updating wallet connection to database",
                    },
                };
            }

            this.logger.LogDebug($"Finished updating wallet connection to database. user {hdId}: {JsonSerializer.Serialize(dbResult)}");
            return new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = WalletConnectionModel.CreateFromDbModel(dbResult.Payload),
                ResultStatus = ResultType.Success,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<WalletCredentialModel>>> CreateCredentialsAsync(string hdId, IEnumerable<string> targetIds)
        {
            this.logger.LogDebug($"Creating credentials {JsonSerializer.Serialize(targetIds)} for user {hdId}");
            List<WalletCredentialModel> resultList = new List<WalletCredentialModel>();
            this.logger.LogDebug($"Getting current connection from database");
            DBResult<WalletConnection> walletConnectionResult = this.walletDelegate.GetCurrentConnection(hdId);
            if (walletConnectionResult.Status != DBStatusCode.Read)
            {
                this.logger.LogDebug($"Error getting current connection from database {JsonSerializer.Serialize(walletConnectionResult)}");
                return new RequestResult<IEnumerable<WalletCredentialModel>>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving wallet connection from database",
                    },
                };
            }

            this.logger.LogDebug($"Finished getting wallet connection from database {JsonSerializer.Serialize(walletConnectionResult)}");
            this.logger.LogDebug($"Getting patient info from client registries {hdId}");
            RequestResult<PatientModel> patientRequestResult =
                await this.clientRegistriesDelegate.GetDemographicsByHDIDAsync(hdId).ConfigureAwait(true);
            if (patientRequestResult.ResultStatus != ResultType.Success)
            {
                this.logger.LogDebug($"Error getting patient info {hdId}: {JsonSerializer.Serialize(patientRequestResult)}");
                return new RequestResult<IEnumerable<WalletCredentialModel>>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error getting patient info when creating wallet credential",
                        InnerError = patientRequestResult.ResultError,
                    },
                };
            }

            this.logger.LogDebug($"Finished getting patient info {JsonSerializer.Serialize(patientRequestResult)}");
            foreach (string targetId in targetIds)
            {
                this.logger.LogDebug($"Getting immunization info {targetId}");
                RequestResult<ImmunizationEvent> immunizationResult =
                    await this.immunizationDelegate.GetImmunization(hdId, targetId).ConfigureAwait(true);
                if (immunizationResult.ResultStatus != ResultType.Success)
                {
                    this.logger.LogDebug($"Error getting immunization info {targetId}: {JsonSerializer.Serialize(immunizationResult)}");
                    return new RequestResult<IEnumerable<WalletCredentialModel>>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError()
                        {
                            ResultMessage = "Error getting immunization info when creating wallet credential",
                            InnerError = patientRequestResult.ResultError,
                        },
                    };
                }

                this.logger.LogDebug($"Finished getting immunization info {targetId}: {JsonSerializer.Serialize(immunizationResult)}");
                ImmunizationEvent immunization = immunizationResult.ResourcePayload!;
                ImmunizationCredentialPayload credentialPayload = new ImmunizationCredentialPayload()
                {
                    Provider = immunization.ProviderOrClinic,
                    LotNumber = string.Join(",", immunization.Immunization.ImmunizationAgents.Select(immz => immz.LotNumber)),
                    ImmunizationType = immunization.Immunization.Name,
                    ImmunizationProduct = string.Join(",", immunization.Immunization.ImmunizationAgents.Select(immz => immz.ProductName)),
                    ImmunizationAgent = string.Join(",", immunization.Immunization.ImmunizationAgents.Select(immz => immz.Name)),
                    RecipientBirthDate = patientRequestResult.ResourcePayload!.Birthdate,
                    RecipientName = $"{patientRequestResult.ResourcePayload.FirstName} {patientRequestResult.ResourcePayload.LastName}",
                    RecipientPHN = patientRequestResult.ResourcePayload.PersonalHealthNumber,
                };

                this.logger.LogDebug($"Creating wallet credential with issuer {JsonSerializer.Serialize(credentialPayload)}");
                RequestResult<CredentialResponse> walletIssuerCredentialResult =
                    await this.walletIssuerDelegate.CreateCredentialAsync(walletConnectionResult.Payload, credentialPayload, CredentialComment).ConfigureAwait(true);

                if (walletIssuerCredentialResult.ResultStatus != ResultType.Success)
                {
                    this.logger.LogDebug($"Error creating wallet credential with issuer {JsonSerializer.Serialize(walletIssuerCredentialResult)}");
                    return new RequestResult<IEnumerable<WalletCredentialModel>>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError()
                        {
                            ResultMessage = "Error creating wallet credential with wallet issuer",
                            InnerError = walletIssuerCredentialResult.ResultError,
                        },
                    };
                }

                this.logger.LogDebug($"Finished creating wallet credential with issuer {JsonSerializer.Serialize(walletIssuerCredentialResult)}");
                WalletCredential walletCredential = new WalletCredential()
                {
                    ExchangeId = walletIssuerCredentialResult.ResourcePayload!.ExchangeId,
                    ResourceId = targetId,
                    ResourceType = ImmunizationResourceType,
                    WalletConnectionId = walletConnectionResult.Payload.Id,
                    Status = WalletCredentialStatus.Created,
                };

                this.logger.LogDebug($"Inserting wallet credential to database {JsonSerializer.Serialize(walletCredential)}");
                DBResult<WalletCredential> walletCredentialResult = this.walletDelegate.InsertCredential(walletCredential);
                if (walletCredentialResult.Status != DBStatusCode.Created)
                {
                    this.logger.LogDebug($"Error inserting wallet credential to database {JsonSerializer.Serialize(walletCredentialResult)}");
                    return new RequestResult<IEnumerable<WalletCredentialModel>>()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new RequestResultError()
                        {
                            ResultMessage = "Error inserting wallet connection to database",
                        },
                    };
                }

                this.logger.LogDebug($"Finished inserting wallet credential to database {JsonSerializer.Serialize(walletCredentialResult)}");
                resultList.Add(WalletCredentialModel.CreateFromDbModel(walletCredentialResult.Payload));
            }

            this.logger.LogDebug($"Finished creating credentials {JsonSerializer.Serialize(targetIds)} for user {hdId}: {JsonSerializer.Serialize(resultList)}");
            return new RequestResult<IEnumerable<WalletCredentialModel>>()
            {
                ResourcePayload = resultList,
                ResultStatus = ResultType.Success,
            };
        }

        /// <inheritdoc/>
        public RequestResult<WalletConnectionModel> GetConnection(string hdId)
        {
            this.logger.LogDebug($"Getting wallet connection from database for user {hdId}");
            DBResult<WalletConnection> dbResult = this.walletDelegate.GetCurrentConnection(hdId);
            if (dbResult.Status != DBStatusCode.Read)
            {
                this.logger.LogDebug($"Error getting wallet connection from database. {JsonSerializer.Serialize(dbResult)}");
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving wallet connection from database",
                    },
                };
            }

            this.logger.LogDebug($"Finished getting wallet connection from database. {JsonSerializer.Serialize(dbResult)}");
            return new RequestResult<WalletConnectionModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = WalletConnectionModel.CreateFromDbModel(dbResult.Payload),
            };
        }

        /// <inheritdoc/>
        public RequestResult<WalletCredentialModel> GetCredential(Guid exchangeId)
        {
            this.logger.LogDebug($"Getting wallet credential from database. credential exchange id: {exchangeId}");
            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredential(exchangeId);
            if (dbResult.Status != DBStatusCode.Read)
            {
                this.logger.LogDebug($"Error getting wallet credential from database. {JsonSerializer.Serialize(dbResult)}");
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving wallet credential from database",
                    },
                };
            }

            this.logger.LogDebug($"Finished getting wallet credential from database. {JsonSerializer.Serialize(dbResult)}");
            return new RequestResult<WalletCredentialModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = WalletCredentialModel.CreateFromDbModel(dbResult.Payload),
            };
        }
    }
}
