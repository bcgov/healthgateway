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
namespace HealthGateway.Admin.Client.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models.AdminReports;
    using HealthGateway.Common.Data.Constants;
    using Refit;

    /// <summary>
    /// API for retrieving reports.
    /// </summary>
    public interface IAdminReportApi
    {
        /// <summary>
        /// Retrieves a collection of user HDIDs and their blocked data sources.
        /// </summary>
        /// <returns>A collection of <see cref="BlockedAccessRecord"/> records.</returns>
        [Get("/BlockedAccess")]
        Task<IEnumerable<BlockedAccessRecord>> GetBlockedAccessReportAsync();

        /// <summary>
        /// Retrieves a collection of user HDIDs that have dependents.
        /// </summary>
        /// <param name="page">Page number of the protected dependents report (First page is zero).</param>
        /// <param name="pageSize">Number or records per page to return from the protected dependents report.</param>
        /// <param name="sortDirection">The sort direction for the records in the protected dependents report.</param>
        /// <returns>Collection of user HDIDs that have dependents attached.</returns>
        [Get("/ProtectedDependents")]
        Task<ProtectedDependentReport> GetProtectedDependentsReportAsync(int? page = 0, int? pageSize = 25, SortDirection? sortDirection = SortDirection.Ascending);
    }
}
