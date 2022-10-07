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
namespace HealthGateway.Admin.Controllers
{
    using HealthGateway.Admin.Constants;
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user support requests.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class SupportController
    {
        private readonly ISupportService supportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportController"/> class.
        /// </summary>
        /// <param name="supportService">The injected support service.</param>
        public SupportController(ISupportService supportService)
        {
            this.supportService = supportService;
        }

        /// <summary>
        /// Retrieves a list of users matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A list of users matching the query.</returns>
        /// <response code="200">Returns the list of users matching the query.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("Users")]
        public IActionResult GetSupportUsers([FromQuery] UserQueryType queryType, [FromQuery] string queryString)
        {
            return new JsonResult(this.supportService.GetSupportUsers(queryType, queryString));
        }

        /// <summary>
        /// Retrieves a list of message verifications matching the query.
        /// </summary>
        /// <param name="hdid">The hdid associated with the messaging verification.</param>
        /// <returns>A list of users matching the query.</returns>
        /// <response code="200">Returns the list of users matching the query.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("Verifications")]
        public IActionResult GetMessageVerifications([FromQuery] string hdid)
        {
            return new JsonResult(this.supportService.GetMessageVerifications(hdid));
        }
    }
}
