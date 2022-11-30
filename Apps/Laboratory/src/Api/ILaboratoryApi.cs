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
namespace HealthGateway.Laboratory.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface that defines a client api to  retrieve laboratory information.
    /// </summary>
    public interface ILaboratoryApi
    {
        /// <summary>
        /// Returns a List of COVID-19 Orders for the authenticated user.
        /// It has a collection of one or more COVID-19 results depending on the tests ordered.
        /// </summary>
        /// <param name="query">Query parameters used to query COVID-19 results.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The list of COVID-19 Orders available for the user identified by the bearer token.</returns>
        [Get("/api/v1/LabOrders")]
        Task<PhsaResult<List<PhsaCovid19Order>>> GetCovid19OrdersAsync(Dictionary<string, string?> query, [Authorize] string token);

        /// <summary>
        /// Returns the laboratory report for the provided laboratory order.
        /// </summary>
        /// <param name="id">The id of the laboratory report to return.</param>
        /// <param name="query">Query parameters used to query the laboratory report.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The laboratory report identified by the id and query parameters.</returns>
        [Get("/api/v1/LabOrders/{id}/LabReportDocument")]
        Task<LaboratoryReport> GetLaboratoryReportAsync(string id, Dictionary<string, string?> query, [Authorize] string token);

        /// <summary>
        /// Returns the plis pdf document for the provided laboratory order.
        /// </summary>
        /// <param name="id">The id of the plis pdf document to return.</param>
        /// <param name="query">Query parameters used to query the plis pdf document.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The plis pdf document identified by the id and query parameters.</returns>
        [Get("/api/v1/Lab/Plis/{id}/LabReportDocument")]
        Task<LaboratoryReport> GetPlisLaboratoryReportAsync(string id, Dictionary<string, string?> query, [Authorize] string token);

        /// <summary>
        /// Returns the provincial lab information system laboratory summary belonging to the authenticated user.
        /// </summary>
        /// <param name="query">Query parameters used to query the plis laboratory summary.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The plis laboratory summary identified by the query parameters.</returns>
        [Get("/api/v1/Lab/Plis/LabSummary")]
        Task<PhsaResult<PhsaLaboratorySummary>> GetPlisLaboratorySummaryAsync(Dictionary<string, string?> query, [Authorize] string token);

        /// <summary>
        /// Returns the public COVID-19 test results for the given patient.
        /// </summary>
        /// <param name="query">Query parameters used to query the public covid test results.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <param name="clientIp">The ip of the client to send.</param>
        /// <returns>The public covid test results identified by the query parameters.</returns>
        [Post("/api/v1/Public/LabOrders/Covid19LabSummary")]
        Task<PhsaResult<IEnumerable<CovidTestResult>>> GetPublicCovidLabSummaryAsync(
            [Body] Dictionary<string, string?> query,
            [Authorize] string token,
            [Header("X-Forwarded-For")] string clientIp);
    }
}
