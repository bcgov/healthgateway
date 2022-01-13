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
namespace HealthGateway.Laboratory.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Filters;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The authenticated laboratory controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class LaboratoryController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Gets or sets the laboratory data service.
        /// </summary>
        private readonly ILaboratoryService service;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The laboratory data service.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public LaboratoryController(
            ILogger<LaboratoryController> logger,
            ILaboratoryService svc,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.service = svc;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets a json list of laboratory orders.
        /// </summary>
        /// <param name="hdid">The hdid resource to request the laboratory orders for.</param>
        /// <returns>A list of laboratory records wrapped in a request result.</returns>
        /// <response code="200">Returns the List of laboratory records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<IActionResult> GetLaboratoryOrders([FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting list of laboratory orders... ");

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    RequestResult<IEnumerable<LaboratoryModel>> result = await this.service.GetLaboratoryOrders(accessToken, hdid).ConfigureAwait(true);
                    this.logger.LogDebug($"Finished getting lab orders from controller... {hdid}");

                    return new JsonResult(result);
                }
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Gets a a specific Laboratory report.
        /// </summary>
        /// <param name="reportId">The ID of the report belonging to the authenticated user to fetch.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <returns>A Laboratory PDF Report wrapped in a request result.</returns>
        /// <response code="200">Returns the specified PDF lab report.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{reportId}/Report")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<IActionResult> GetLaboratoryReport(Guid reportId, [FromQuery] string hdid)
        {
            this.logger.LogDebug($"Getting PDF version of Laboratory Report for hdid {hdid}");

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    RequestResult<LaboratoryReport> result = await this.service.GetLabReport(reportId, hdid, accessToken).ConfigureAwait(true);
                    this.logger.LogDebug($"Finished getting pdf report from controller... {hdid}");

                    return new JsonResult(result);
                }
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Post a rapid test.
        /// </summary>
        /// <param name="hdid">The requested HDID which owns the rapid test request.</param>
        /// <param name="rapidTestRequest">The rapid test request model.</param>
        /// <returns>A Rapid Test Result object wrapped in a request result.</returns>
        /// <response code="200">Return the Submission status is completed successfully.</response>
        /// <response code="403">DID Claim is missing or can not resolve PHN.</response>
        /// <response code="409">Combination of Phn and Serial number already exists.</response>
        [HttpPost]
        [Produces("application/json")]
        [Route("{hdid}/rapidTest")]
        [Authorize(Policy = LaboratoryPolicy.Write)]
        public async Task<RequestResult<AuthenticatedRapidTestResponse>> CreateRapidTestAsync(string hdid, [FromBody] AuthenticatedRapidTestRequest rapidTestRequest)
        {
            RequestResult<AuthenticatedRapidTestResponse> result = new();
            this.logger.LogDebug($"Post rapid test for hdid {hdid}");
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (accessToken != null)
                {
                    result = await this.service.CreateRapidTestAsync(hdid, accessToken, rapidTestRequest).ConfigureAwait(true);
                    this.logger.LogDebug($"Finished submitting a rapid test from controller... {hdid}");
                    return result;
                }
            }

            return result;
        }
    }
}
