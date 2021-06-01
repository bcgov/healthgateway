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
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Delegates;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Models.AcaPy;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class WalletService : IWalletService
    {
        private readonly ILogger logger;
        private readonly IWalletDelegate walletDelegate;
        private readonly IWalletIssuerDelegate walletIssuerDelegate;
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="walletDelegate">Injected Wallet delegate.</param>
        /// <param name="walletIssuerDelegate">Injected Wallet issuer delegate.</param>
        /// <param name="clientRegistriesDelegate">Injected client registries delegate.</param>
        public WalletService(
            ILogger<WalletService> logger,
            IWalletDelegate walletDelegate,
            IWalletIssuerDelegate walletIssuerDelegate,
            IClientRegistriesDelegate clientRegistriesDelegate)
        {
            this.logger = logger;
            this.walletDelegate = walletDelegate;
            this.walletIssuerDelegate = walletIssuerDelegate;
            this.clientRegistriesDelegate = clientRegistriesDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<WalletConnectionModel>> CreateConnectionAsync(string hdId)
        {
            WalletConnection walletConnection = new WalletConnection();
            walletConnection.Id = Guid.NewGuid();
            walletConnection.UserProfileId = hdId;

            RequestResult<ConnectionResponse> walletIssuerConnection =
                await this.walletIssuerDelegate.CreateConnectionAsync(walletConnection.Id).ConfigureAwait(true);

            if (walletIssuerConnection.ResultStatus != ResultType.Success)
            {
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error creating wallet connection with wallet issuer",
                        InnerError = walletIssuerConnection.ResultError,
                    },
                };
            }

            walletConnection.InvitationEndpoint = walletIssuerConnection.ResourcePayload!.InvitationUrl.AbsoluteUri;
            walletConnection.AgentId = walletIssuerConnection.ResourcePayload!.AgentId;
            walletConnection.Status = WalletConnectionStatus.Pending;
            DBResult<WalletConnection> dbResult = this.walletDelegate.InsertConnection(walletConnection);
            if (dbResult.Status != DBStatusCode.Created)
            {
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error inserting wallet connection to database",
                    },
                };
            }

            return new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = WalletConnectionModel.CreateFromDbModel(dbResult.Payload),
                ResultStatus = ResultType.Success,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<WalletCredentialModel>> CreateCredentialAsync(string hdId, string targetId)
        {
            // TODO: Call to Immz to get payload data.
            RequestResult<PatientModel> patientRequestResult =
                await this.clientRegistriesDelegate.GetDemographicsByHDIDAsync(hdId).ConfigureAwait(true);
            if (patientRequestResult.ResultStatus != ResultType.Success)
            {
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error getting patient info when creating wallet credential",
                        InnerError = patientRequestResult.ResultError,
                    },
                };
            }

            ImmunizationCredentialPayload credentialPayload = new ImmunizationCredentialPayload()
            {
                // TODO: Fill up immz data
                Provider = string.Empty,
                LotNumber = string.Empty,
                ImmunizationType = string.Empty,
                ImmunizationProduct = string.Empty,
                ImmunizationAgent = string.Empty,
                RecipientBirthDate = patientRequestResult.ResourcePayload!.Birthdate,
                RecipientName = $"{patientRequestResult.ResourcePayload.FirstName} {patientRequestResult.ResourcePayload.LastName}",
                RecipientPHN = patientRequestResult.ResourcePayload.PersonalHealthNumber,
            };

            DBResult<WalletConnection> walletConnection = this.walletDelegate.GetConnection(hdId);
            if (walletConnection.Status != DBStatusCode.Read)
            {
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error getting wallet connection from database",
                    },
                };
            }

            RequestResult<CredentialResponse> walletIssuerCredential =
                await this.walletIssuerDelegate.CreateCredentialAsync(walletConnection.Payload, credentialPayload).ConfigureAwait(true);

            if (walletIssuerCredential.ResultStatus != ResultType.Success)
            {
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error creating wallet credential with wallet issuer",
                        InnerError = walletIssuerCredential.ResultError,
                    },
                };
            }

            WalletCredential walletCredential = new WalletCredential()
            {
                Id = Guid.NewGuid(),
                ExchangeId = walletIssuerCredential.ResourcePayload.ExchangeId,
                ResourceId = targetId,
                WalletConnectionId = walletConnection.Payload.Id,
                Status = WalletCredentialStatus.Created,
            };

            DBResult<WalletCredential> dbResult = this.walletDelegate.InsertCredential(walletCredential);
            if (dbResult.Status != DBStatusCode.Created)
            {
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error inserting wallet connection to database",
                    },
                };
            }

            return new RequestResult<WalletCredentialModel>()
            {
                ResourcePayload = WalletCredentialModel.CreateFromDbModel(dbResult.Payload),
                ResultStatus = ResultType.Success,
            };
        }

        /// <inheritdoc/>
        public RequestResult<WalletConnectionModel> GetConnection(string hdId)
        {
            DBResult<WalletConnection> dbResult = this.walletDelegate.GetConnection(hdId);
            if (dbResult.Status != DBStatusCode.Read)
            {
                return new RequestResult<WalletConnectionModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving wallet connection from database",
                    },
                };
            }

            return new RequestResult<WalletConnectionModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = WalletConnectionModel.CreateFromDbModel(dbResult.Payload),
            };
        }

        /// <inheritdoc/>
        public RequestResult<WalletCredentialModel> GetCredential(Guid exchangeId)
        {
            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredential(exchangeId);
            if (dbResult.Status != DBStatusCode.Read)
            {
                return new RequestResult<WalletCredentialModel>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = "Error retrieving wallet credential from database",
                    },
                };
            }

            return new RequestResult<WalletCredentialModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = WalletCredentialModel.CreateFromDbModel(dbResult.Payload),
            };
        }
    }
}
