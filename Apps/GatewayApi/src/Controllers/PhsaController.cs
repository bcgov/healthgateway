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

namespace HealthGateway.GatewayApi.Controllers
{
    using System.Collections.Generic;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle dependent interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class PhsaController
    {
        private readonly IDependentService dependentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhsaController"/> class.
        /// </summary>
        /// <param name="dependentService">The injected dependent service.</param>
        public PhsaController(IDependentService dependentService)
        {
            this.dependentService = dependentService;
        }

        /// <summary>
        /// Gets all dependents for the specified user.
        /// </summary>
        /// <returns>The list of dependent model wrapped in a request result.</returns>
        /// <param name="hdid">The owner hdid.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [Route("{hdid}")]
        public RequestResult<IEnumerable<DependentModel>> GetAll(string hdid)
        {
            return this.dependentService.GetDependents(hdid);
        }

        /// <summary>
        /// Gets all dependents for the specified date range.
        /// </summary>
        /// <returns>The list of dependents wrapped in a request result.</returns>
        /// <param name="fromDateUtc">The from date in Utc. Required.</param>
        /// <param name="toDateUtc">The to date in Utc.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size. Max 5000.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [Route("dependents/{fromDate}")]
        public RequestResult<IEnumerable<GetDependentResponse>> GetAll(string fromDateUtc, [FromQuery] string toDateUtc, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            return this.dependentService.GetDependents(fromDateUtc, toDateUtc, pageNumber, pageSize);
        }
    }
}
