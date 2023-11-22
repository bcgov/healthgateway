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
    /// The delegate service.
    /// </summary>
    public interface IDelegateService
    {
        /// <summary>
        /// Creates a delegation for the given delegator's hdid.
        /// </summary>
        /// <param name="hdid">The delegator's hdid.</param>
        /// <param name="request">The create delegation request model.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<string> CreateDelegationAsync(string hdid, CreateDelegationRequest request, CancellationToken ct = default);
    }
}
