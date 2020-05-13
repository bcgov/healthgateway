﻿//-------------------------------------------------------------------------
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
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Immunization controller.
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
        /// Gets or sets the authorization service.
        /// </summary>
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="svc">The immunization data service.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        /// <param name="authorizationService">The IAuthorizationService.</param>
        public LaboratoryController(
            ILogger<LaboratoryController> logger,
            ILaboratoryService svc,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            this.logger = logger;
            this.service = svc;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets a json list of laboratory orders.
        /// </summary>
        /// <returns>A list of laboratory records wrapped in a request result.</returns>
        /// <response code="200">Returns the List of laboratory records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetLaboratoryOrders()
        {
            this.logger.LogDebug($"Getting list of laboratory orders... ");

            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string hdid = user.FindFirst("hdid").Value;
            string accessToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);
            var isAuthorized = await this.authorizationService.AuthorizeAsync(user, hdid, PolicyNameConstants.UserIsPatient).ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            RequestResult<IEnumerable<LaboratoryOrder>> result = await this.service.GetLaboratoryOrders(accessToken).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting lab orders from controller... {hdid}");

            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a a specific Laboratory report.
        /// </summary>
        /// <param name="reportId">The ID of the report belonging to the authenticated user to fetch.</param>
        /// <returns>A Laboratory PDF Report wrapped in a request result.</returns>
        /// <response code="200">Returns the specified PDF lab report.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{reportId}/Report")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> GetLaboratoryReport(Guid reportId)
        {
            this.logger.LogDebug($"Getting PDF version of Laboratory Report... {1}");

            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string hdid = user.FindFirst("hdid").Value;
            string accessToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);
            var isAuthorized = await this.authorizationService.AuthorizeAsync(user, hdid, PolicyNameConstants.UserIsPatient).ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            RequestResult<LaboratoryReport> result = await this.service.GetLabReport(reportId, accessToken).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting pdf report from controller... {hdid}");

            return new JsonResult(result);
        }
    }
}