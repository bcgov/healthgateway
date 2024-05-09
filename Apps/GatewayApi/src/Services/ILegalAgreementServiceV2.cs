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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The Legal Agreement service.
    /// </summary>
    public interface ILegalAgreementServiceV2
    {
        /// <summary>
        /// Gets the most recent active terms of service.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped terms of service.</returns>
        Task<RequestResult<TermsOfServiceModel>> GetActiveTermsOfServiceAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets the id of the active legal agreement.
        /// </summary>
        /// <param name="type">The legal agreement type to query on.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The id of the active legal agreement; null if no agreement found.</returns>
        Task<Guid?> GetActiveLegalAgreementId(LegalAgreementType type, CancellationToken ct = default);
    }
}
