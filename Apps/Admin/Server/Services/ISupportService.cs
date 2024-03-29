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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Service that provides functionality to admin support.
    /// </summary>
    public interface ISupportService
    {
        /// <summary>
        /// Retrieves patient support details, which may include messaging verifications, agent actions, blocked data sources,
        /// dependents, and COVID-19 details.
        /// </summary>
        /// <param name="query">Query model.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>Patient support details matching the query.</returns>
        Task<PatientSupportDetails> GetPatientSupportDetailsAsync(PatientSupportDetailsQuery query, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the collection of patients that match the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>The collection of patient support details that match the query.</returns>
        Task<IReadOnlyList<PatientSupportResult>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default);

        /// <summary>
        /// Block access to data sources associated with the hdid.
        /// </summary>
        /// <param name="hdid">The hdid that is associated with the data sources.</param>
        /// <param name="dataSources">The list of data sources that will be blocked.</param>
        /// <param name="reason">The reason to block access to data source(s)..</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task BlockAccessAsync(string hdid, IEnumerable<DataSource> dataSources, string reason, CancellationToken ct = default);
    }
}
