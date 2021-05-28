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
        public void UpdateWalletConnection(Guid connectionId)
        {
            DBResult<WalletConnection> dbResult = this.walletDelegate.GetConnection(connectionId);
            if (dbResult.Status == DBStatusCode.Read)
            {
                WalletConnection connection = dbResult.Payload;
                connection.ConnectedDateTime = DateTime.UtcNow;
                connection.Status = WalletConnectionStatus.Connected;
                this.walletDelegate.UpdateConnection(connection, true);
            }
            else
            {
                this.logger.LogWarning($"Unable to find wallet connection with id: {connectionId}");
            }
        }

        /// <inheritdoc />
        public void UpdateWalletCredential(Guid exchangeId)
        {
            DBResult<WalletCredential> dbResult = this.walletDelegate.GetCredential(exchangeId);
            if (dbResult.Status == DBStatusCode.Read)
            {
                WalletCredential credential = dbResult.Payload;
                credential.AddedDateTime = DateTime.UtcNow;
                credential.Status = WalletCredentialStatus.Added;
                this.walletDelegate.UpdateCredential(credential, true);
            }
            else
            {
                this.logger.LogWarning($"Unable to find wallet credential using exchange id: {exchangeId}");
            }
        }
    }
}
