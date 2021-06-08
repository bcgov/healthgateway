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
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;

    /// <summary>
    /// Service to interact with the Wallet connections and credentials.
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Creates a wallet connection in the backend.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>The created wallet connection model wrapped in a RequestResult.</returns>
        Task<RequestResult<WalletConnectionModel>> CreateConnectionAsync(string hdId);

        /// <summary>
        /// Creates a wallet credential in the backend.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <param name="targetId">The target id to be added.</param>
        /// <returns>The created wallet credential model wrapped in a RequestResult.</returns>
        Task<RequestResult<WalletCredentialModel>> CreateCredentialAsync(string hdId, string targetId);

        /// <summary>
        /// Creates a wallet credential in the backend.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <param name="targetIds">The target id to be added to the wallet credential.</param>
        /// <returns>The created wallet credential model wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<WalletCredentialModel>>> CreateCredentialsAsync(string hdId, IEnumerable<string> targetIds);

        /// <summary>
        /// Gets the wallet connection for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <returns>A wallet connection model wrapped in a RequestResult.</returns>
        RequestResult<WalletConnectionModel> GetConnection(string hdId);

        /// <summary>
        /// Gets the wallet credentials.
        /// </summary>
        /// <param name="exchangeId">The wallet credential exchange id.</param>
        /// <returns>A wallet credential model wrapped in a RequestResult.</returns>
        RequestResult<WalletCredentialModel> GetCredentialByExchangeId(Guid exchangeId);

        /// <summary>
        /// Disconnects the identified wallet.
        /// </summary>
        /// <param name="connectionId">The wallet connection id.</param>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>A wallet credential model wrapped in a RequestResult.</returns>
        Task<RequestResult<WalletConnectionModel>> DisconnectConnection(Guid connectionId, string hdId);

        /// <summary>
        /// Revokes the identified wallet credential if in created or added state.
        /// </summary>
        /// <param name="credentialId">The wallet credential id.</param>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>A wallet credential model wrapped in a RequestResult.</returns>
        Task<RequestResult<WalletCredentialModel>> RevokeCredential(Guid credentialId, string hdId);

        /// <summary>
        /// Revokes the identified wallet credential if in created or added state.
        /// </summary>
        /// <param name="credential">The wallet credential id.</param>
        /// <returns>A wallet credential model wrapped in a RequestResult.</returns>
        Task<RequestResult<WalletCredentialModel>> RevokeCredential(WalletCredential credential);
    }
}
