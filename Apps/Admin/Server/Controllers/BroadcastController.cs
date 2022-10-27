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
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle system broadcasts.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class BroadcastController
    {
        private readonly IBroadcastService broadcastService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastController"/> class.
        /// </summary>
        /// <param name="broadcastService">The injected broadcast service.</param>
        public BroadcastController(IBroadcastService broadcastService)
        {
            this.broadcastService = broadcastService;
        }

        /// <summary>
        /// Creates a broadcast.
        /// </summary>
        /// <returns>The created broadcast wrapped in a RequestResult.</returns>
        /// <param name="broadcast">The broadcast model.</param>
        /// <response code="200">Returns the created broadcast wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        public async Task<RequestResult<Broadcast>> CreateBroadcast(Broadcast broadcast)
        {
            return await this.broadcastService.CreateBroadcastAsync(broadcast).ConfigureAwait(true);
        }

        /// <summary>
        /// Retrieves all broadcasts.
        /// </summary>
        /// <returns>The collection of broadcasts wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the collection of broadcasts wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        public async Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcasts()
        {
            return await this.broadcastService.GetBroadcastsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Updates a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <returns>The updated broadcast wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the updated broadcast  wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        public async Task<RequestResult<Broadcast>> UpdateBroadcast(Broadcast broadcast)
        {
            return await this.broadcastService.UpdateBroadcastAsync(broadcast).ConfigureAwait(true);
        }
    }
}
