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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The data access service.
    /// </summary>
    public interface IDataAccessService
    {
        /// <summary>
        /// Gets all data sources that are blocked for the specified user.
        /// </summary>
        /// <param name="hdid">The user's HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The <see cref="BlockedDatasets"/> for the specified user.</returns>
        Task<BlockedDatasets> GetBlockedDatasetsAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Gets contact info for the specified user.
        /// </summary>
        /// <param name="hdid">The user's HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="ContactInfo"/> object for the specified user.</returns>
        Task<ContactInfo> GetContactInfoAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Confirms if the supplied user is protected.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="delegateHdid">The delegate's HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// A <see cref="UserProtection"/> model containing the specified user's HDID and the protection status of the subject
        /// user.
        /// </returns>
        Task<UserProtection> GetUserProtectionAsync(string hdid, string delegateHdid, CancellationToken ct = default);
    }
}
