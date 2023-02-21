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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle patient notes interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IWebAlertService webAlertService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="webAlertService">The injected web alert service.</param>
        public NotificationController(IWebAlertService webAlertService)
        {
            this.webAlertService = webAlertService;
        }

        /// <summary>
        /// Gets all notifications for a user.
        /// </summary>
        /// <param name="hdid">The HDID of the user.</param>
        /// <returns>The collection of notifications.</returns>
        /// <response code="200">Returns the collection of notifications.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WebAlert>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(string hdid)
        {
            IList<WebAlert> notifications = await this.webAlertService.GetWebAlertsAsync(hdid).ConfigureAwait(true);
            return this.Ok(notifications);
        }

        /// <summary>
        /// Dismisses all notifications for a user.
        /// </summary>
        /// <returns>An empty result.</returns>
        /// <param name="hdid">The HDID of the user.</param>
        /// <response code="200">The notifications were dismissed.</response>
        /// <response code="401">The client must authenticate itself to perform the operation.</response>
        /// <response code="403">
        /// The client does not have access rights to perform the operation; that is, it is unauthorized.
        /// Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DismissAll(string hdid)
        {
            await this.webAlertService.DismissWebAlertsAsync(hdid).ConfigureAwait(true);
            return this.Ok();
        }

        /// <summary>
        /// Dismisses a notification for a user.
        /// </summary>
        /// <returns>An empty result.</returns>
        /// <param name="hdid">The HDID of the user.</param>
        /// <param name="id">The ID of the notification to be dismissed.</param>
        /// <response code="200">The notification was dismissed.</response>
        /// <response code="401">The client must authenticate itself to perform the operation.</response>
        /// <response code="403">
        /// The client does not have access rights to perform the operation; that is, it is unauthorized.
        /// Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        [Route("{hdid}/{id:guid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Dismiss(string hdid, Guid id)
        {
            await this.webAlertService.DismissWebAlertAsync(hdid, id).ConfigureAwait(true);
            return this.Ok();
        }
    }
}
