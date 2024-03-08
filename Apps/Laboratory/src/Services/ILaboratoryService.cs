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
namespace HealthGateway.Laboratory.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Laboratory.Models;

    /// <summary>
    /// The laboratory data service.
    /// </summary>
    public interface ILaboratoryService
    {
        /// <summary>
        /// Returns result containing a collection of COVID-19 lab orders for the authenticated user.
        /// Each order has a collection of one or more COVID-19 results depending on the tests ordered.
        /// </summary>
        /// <param name="hdid">The requested hdid.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The collection of COVID-19 lab orders available for the authenticated user.</returns>
        Task<RequestResult<Covid19OrderResult>> GetCovid19OrdersAsync(string hdid, int pageIndex = 0, CancellationToken ct = default);

        /// <summary>
        /// Returns result containing a collection of lab orders for the authenticated user.
        /// Each order has a collection of one or more lab results depending on the tests ordered.
        /// </summary>
        /// <param name="hdid">The requested hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns collection of lab orders available for the authenticated user.</returns>
        Task<RequestResult<LaboratoryOrderResult>> GetLaboratoryOrdersAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Gets the Lab report for the supplied id belonging to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the lab report to get.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="isCovid19">Indicates whether the COVID-19 report should be returned..</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A base64 encoded PDF.</returns>
        Task<RequestResult<LaboratoryReport>> GetLabReportAsync(string id, string hdid, bool isCovid19, CancellationToken ct = default);
    }
}
