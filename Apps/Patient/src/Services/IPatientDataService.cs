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
namespace HealthGateway.Patient.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Patient.Models;

    /// <summary>
    /// Provides access to patient related data services.
    /// </summary>
    public interface IPatientDataService
    {
        /// <summary>
        /// Query data services for health options.
        /// </summary>
        /// <param name="query">The query message.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The Response message.</returns>
        Task<PatientDataResponse> Query(PatientDataOptionsQuery query, CancellationToken ct);

        /// <summary>
        /// Query patient files.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Patient file or null if not found.</returns>
        Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct);

        /// <summary>
        /// Query data services for health data.
        /// </summary>
        /// <param name="query">The query message.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Patient data in the response message.</returns>
        Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct);
    }
}
