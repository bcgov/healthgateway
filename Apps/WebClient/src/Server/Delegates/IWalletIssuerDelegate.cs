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
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models.AcaPy;

    /// <summary>
    /// Interface that defines a delegate to create/revoke wallet connections and credentials.
    /// </summary>
    public interface IWalletIssuerDelegate
    {
        /// <summary>
        /// Creates a connection invitation request.
        /// </summary>
        /// <param name="walletConnectionId">The id of the wallet connection.</param>
        /// <returns>Create ConnectionResponse including the invitation URL and the agent connection id.</returns>
        Task<RequestResult<CreateConnectionResponse>> CreateConnectionAsync(Guid walletConnectionId);
    }
}
