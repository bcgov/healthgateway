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
namespace HealthGateway.WebClient.Controllers
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Web API to handle dependent interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class DependentController
    {
        private readonly ILogger logger;
        private readonly IDependentService dependentService;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentController"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="dependentService">The injected user feedback service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public DependentController(
            ILogger<UserProfileController> logger,
            IDependentService dependentService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.dependentService = dependentService;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets all dependents for the specified user.
        /// </summary>
        /// <returns>The list of dependent model wrapped in a request result.</returns>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Authorize(Policy = UserPolicy.UserOnly)]
        public IActionResult GetAll()
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            RequestResult<IEnumerable<DependentModel>> result = this.dependentService.GetDependents(userHdid);
            return new JsonResult(result);
        }

        /// <summary>
        /// Posts a Register Dependent Request json to be validated then inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="addDependentRequest">The Register Dependent request model.</param>
        /// <response code="200">The Dependent record was saved.</response>
        /// <response code="400">The Dependent was already inserted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Authorize(Policy = UserPolicy.UserOnly)]
        public IActionResult AddDependent([FromBody] AddDependentRequest addDependentRequest)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string delegateHdId = user.FindFirst("hdid").Value;
            RequestResult<DependentModel> result = this.dependentService.AddDependent(delegateHdId, addDependentRequest);
            return new JsonResult(result);
        }
    }
}
