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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// The gateway communication service.
    /// </summary>
    public interface IGatewayCommunicationService
    {
        /// <summary>
        /// Gets the active communication based on type from the backend.
        /// Only Banner, In-App, and Mobile values are supported.
        /// </summary>
        /// <param name="communicationType">The type of communication to retrieve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The active communication wrapped in a RequestResult.</returns>
        Task<RequestResult<CommunicationModel?>> GetActiveCommunicationAsync(CommunicationType communicationType, CancellationToken ct = default);
    }
}
