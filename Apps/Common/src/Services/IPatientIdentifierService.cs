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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Caching patient identifier service.
    /// </summary>
    public interface IPatientIdentifierService
    {
        /// <summary>
        /// Gets the PHSA patient identifier (PID) using the supplied HDID.
        /// </summary>
        /// <param name="hdid">The HDID to look up.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The PID.</returns>
        Task<Guid> GetPatientIdentifierAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Gets the PHSA patient identifier (PID) using the supplied HDID.
        /// </summary>
        /// <param name="hdid">The HDID to look up.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The PID wrapped in a RequestResult.</returns>
        Task<RequestResult<Guid>> GetPatientIdentifierResultAsync(string hdid, CancellationToken ct = default);
    }
}
