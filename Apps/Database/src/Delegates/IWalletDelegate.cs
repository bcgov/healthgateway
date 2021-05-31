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
namespace HealthGateway.Database.Delegates
{
    using System;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations on the Wallet.
    /// </summary>
    public interface IWalletDelegate
    {
        /// <summary>
        /// Gets the wallet associated by the primary key.
        /// </summary>
        /// <param name="id">The Wallet Connection Id to retrieve.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletConnection> GetConnection(Guid id);

        /// <summary>
        /// Gets the first wallet associated to the user profile Id that isn't in a disconnected state.
        /// </summary>
        /// <param name="userProfileId">The profile id/hdid of the wallet to retrieve.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletConnection> GetCurrentConnection(string userProfileId);

        /// <summary>
        /// Gets the Wallet credentials associated to a given exchange id.
        /// </summary>
        /// <param name="exchangeId">The agent exchange id to query.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletCredential> GetCredential(Guid exchangeId);

        /// <summary>
        /// Creates a WalletConnection object in the database.
        /// </summary>
        /// <param name="connection">The connection to create.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletConnection> InsertConnection(WalletConnection connection, bool commit = true);

        /// <summary>
        /// Updates a WalletConnection object in the database.
        /// </summary>
        /// <param name="connection">The connection to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletConnection> UpdateConnection(WalletConnection connection, bool commit = true);

        /// <summary>
        /// Inserts a WalletCredential object into the database.
        /// </summary>
        /// <param name="credential">The wallet credential to insert.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletCredential> InsertCredential(WalletCredential credential, bool commit = true);

        /// <summary>
        /// Updates a WalletCredential object in the database.
        /// </summary>
        /// <param name="credential">The wallet credential to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<WalletCredential> UpdateCredential(WalletCredential credential, bool commit = true);
    }
}
