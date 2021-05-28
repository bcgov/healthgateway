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
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Models.AcaPy;

    /// <summary>
    /// Service to interact with the Comment Delegate.
    /// </summary>
    public interface IWalletStatusService
    {
        /// <summary>
        /// Updates the state of the Wallet Connection using the connection id.
        /// </summary>
        /// <param name="connectionId">The id of the wallet connection to be updated.</param>
        void UpdateWalletConnection(Guid connectionId);

        /// <summary>
        /// Updates the state of the Wallet Credential using the exchange id.
        /// </summary>
        /// <param name="exchangeId">The exchange id to query the wallet credential.</param>
        void UpdateWalletCredential(Guid exchangeId);
    }
}
