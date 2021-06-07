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
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        public RequestResult<WalletConnection> UpdateWalletConnectionStatus(Guid connectionId, WalletConnectionStatus state)
        {
            RequestResult<WalletConnection> result = new ()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

            DBResult<WalletConnection> dbResult = this.walletDelegate.GetConnection(connectionId, null, true);
            if (dbResult.Status == DBStatusCode.Read)
            {
                WalletConnection connection = dbResult.Payload;
                result.ResourcePayload = connection;
                if (connection.Status != WalletConnectionStatus.Disconnected)
                {
                    connection.ConnectedDateTime = DateTime.UtcNow;
                    connection.Status = state;
                    DBResult<WalletConnection> updateResult = this.walletDelegate.UpdateConnection(connection);
                    if (updateResult.Status == DBStatusCode.Updated)
                    {
                        result.ResultStatus = Common.Constants.ResultType.Success;
                        result.TotalResultCount = 1;
                    }
                    else
                    {
                        this.logger.LogWarning($"Unable to update wallet connection with id: {connectionId}");
                        result.ResultError = new RequestResultError() { ResultMessage = "Error Updating Wallet Connection", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                    }
                }
                else
                {
                    this.logger.LogWarning($"Wallet connection in disconnected state, ignoring update for update wallet connection with id: {connectionId}");
                    result.ResultError = new RequestResultError() { ResultMessage = "Error Unable to update disconnected Wallet Connection", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                }
            }
            else
            {
                this.logger.LogWarning($"Unable to find wallet connection with id: {connectionId}");
                result.ResultError = new RequestResultError() { ResultMessage = "Unable to find Wallet Connection", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
            }

            return result;
        }

        /// <inheritdoc />
        public RequestResult<WalletCredential> UpdateWalletCredentialStatus(Guid exchangeId, WalletCredentialStatus state)
        {
            RequestResult<WalletCredential> result = new ()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredentialByExchangeId(exchangeId);
            if (dbResult.Status == DBStatusCode.Read)
            {
                WalletCredential credential = dbResult.Payload;
                result.ResourcePayload = credential;
                credential.AddedDateTime = DateTime.UtcNow;
                credential.Status = state;
                DBResult<WalletCredential> updateResult = this.walletDelegate.UpdateCredential(credential);
                if (updateResult.Status == DBStatusCode.Updated)
                {
                    result.ResultStatus = Common.Constants.ResultType.Success;
                    result.TotalResultCount = 1;
                }
                else
                {
                    this.logger.LogWarning($"Unable to update wallet credential using exchange id: {exchangeId}");
                    result.ResultError = new RequestResultError() { ResultMessage = "Error Updating Wallet Credential", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                }
            }
            else
            {
                this.logger.LogWarning($"Unable to find wallet credential using exchange id: {exchangeId}");
                result.ResultError = new RequestResultError() { ResultMessage = "Unable to find Wallet Credential", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
            }

            return result;
        }

        /// <inheritdoc />
        public RequestResult<WalletCredential> UpdateWalletCredential(Guid exchangeId, string revocationId, string revocationRegistryId)
        {
            RequestResult<WalletCredential> result = new ()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredentialByExchangeId(exchangeId);
            if (dbResult.Status == DBStatusCode.Read)
            {
                WalletCredential credential = dbResult.Payload;
                result.ResourcePayload = credential;
                credential.RevocationId = revocationId;
                credential.RevocationRegistryId = revocationRegistryId;
                DBResult<WalletCredential> updateResult = this.walletDelegate.UpdateCredential(credential);
                if (updateResult.Status == DBStatusCode.Updated)
                {
                    result.ResultStatus = Common.Constants.ResultType.Success;
                    result.TotalResultCount = 1;
                }
                else
                {
                    this.logger.LogWarning($"Unable to update wallet credential using exchange id: {exchangeId}");
                    result.ResultError = new RequestResultError() { ResultMessage = "Error Updating Wallet Credential", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
                }
            }
            else
            {
                this.logger.LogWarning($"Unable to find wallet credential using exchange id: {exchangeId}");
                result.ResultError = new RequestResultError() { ResultMessage = "Unable to find Wallet Credential", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.WalletIssuer) };
            }

            return result;
        }
    }
}
