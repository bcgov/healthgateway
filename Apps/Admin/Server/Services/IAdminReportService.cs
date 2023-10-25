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
    using HealthGateway.Admin.Common.Models.AdminReports;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to manage admin reports.
    /// </summary>
    public interface IAdminReportService
    {
        /// <summary>
        /// Retrieves a collection of protected dependent HDIDs.
        /// </summary>
        /// <param name="page">Page number of the protected dependents report (First page is zero).</param>
        /// <param name="pageSize">Number or records per page to return from the protected dependents report.</param>
        /// <param name="sortDirection">The sort direction for the records in the protected dependents report.</param>
        /// <param name="ct">Cancellation token to manage async request.</param>
        /// <returns>A collection of HDID strings.</returns>
        Task<ProtectedDependentReport> GetProtectedDependentsReportAsync(int page, int pageSize, SortDirection sortDirection, CancellationToken ct);

        /// <summary>
        /// Retrieves a collection of user HDIDs and their blocked data sources.
        /// </summary>
        /// <param name="ct">Cancellation token to manage async request.</param>
        /// <returns>A collection of <see cref="BlockedAccess"/></returns>
        Task<IEnumerable<BlockedAccessRecord>> GetBlockedAccessReportAsync(CancellationToken ct);
    }
}
