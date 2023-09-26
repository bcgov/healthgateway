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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle admin reports.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class AdminReportController : Controller
    {
        private readonly IAdminReportService adminReportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminReportController"/> class.
        /// </summary>
        /// <param name="adminReportService">then injected admin report service.</param>
        public AdminReportController(IAdminReportService adminReportService)
        {
            this.adminReportService = adminReportService;
        }

        /// <summary>
        /// Retrieves a collection of user HDIDs that have dependents.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>List of user HDIDs that have dependents attached.</returns>
        /// <response code="200">Returns the list of user HDIDs that have dependents attached.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content.</response>
        [HttpGet]
        public async Task<IEnumerable<string>> GetProtectedDependentsReport(CancellationToken ct)
        {
            return await this.adminReportService.GetProtectedDependentsReportAsync(ct);
        }

        /// <summary>
        /// Retrieves a collection of user HDIDs and their blocked data sources.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection of <see cref="BlockedAccessRecord"/> records.</returns>
        /// <response code="200">Returns the collection of hdids with the blocked data sources.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content.</response>
        [HttpGet]
        public async Task<IEnumerable<BlockedAccessRecord>> GetBlockedAccessReport(CancellationToken ct)
        {
            return await this.adminReportService.GetBlockedAccessReportAsync(ct);
        }
    }
}
