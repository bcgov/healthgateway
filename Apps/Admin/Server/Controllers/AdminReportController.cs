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
namespace HealthGateway.Admin.Server.Controllers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models.AdminReports;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle admin reports.
    /// </summary>
    /// <param name="adminReportService">The injected admin report service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class AdminReportController(IAdminReportService adminReportService)
    {
        /// <summary>
        /// Retrieves a collection of user HDIDs and their blocked data sources.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection of <see cref="BlockedAccessRecord"/> records.</returns>
        /// <response code="200">Returns the collection of hdids with the blocked data sources.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content.</response>
        [HttpGet]
        [Route("BlockedAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IEnumerable<BlockedAccessRecord>> BlockedAccessReport(CancellationToken ct)
        {
            return await adminReportService.GetBlockedAccessReportAsync(ct);
        }

        /// <summary>
        /// Retrieves a collection of user HDIDs that have dependents.
        /// </summary>
        /// <param name="page">Page number of the protected dependents report (First page is zero).</param>
        /// <param name="pageSize">Number or records per page to return from the protected dependents report.</param>
        /// <param name="sortDirection">The sort direction for the records in the protected dependents report.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>List of user HDIDs that have dependents attached.</returns>
        /// <response code="200">Returns the list of user HDIDs that have dependents attached.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content.</response>
        [HttpGet]
        [Route("ProtectedDependents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ProtectedDependentReport> ProtectedDependentsReport(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25,
            [FromQuery] SortDirection sortDirection = SortDirection.Ascending,
            CancellationToken ct = default)
        {
            return await adminReportService.GetProtectedDependentsReportAsync(page, pageSize, sortDirection, ct);
        }
    }
}
