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
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle system broadcasts.
    /// </summary>
    /// <param name="broadcastService">The injected broadcast service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class BroadcastController(IBroadcastService broadcastService)
    {
        /// <summary>
        /// Creates a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The created broadcast wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the created broadcast wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        public async Task<RequestResult<Broadcast>> CreateBroadcast(Broadcast broadcast, CancellationToken ct)
        {
            return await broadcastService.CreateBroadcastAsync(broadcast, ct);
        }

        /// <summary>
        /// Retrieves all broadcasts.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The collection of broadcasts wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the collection of broadcasts wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        public async Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcasts(CancellationToken ct)
        {
            return await broadcastService.GetBroadcastsAsync(ct);
        }

        /// <summary>
        /// Updates a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated broadcast wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the updated broadcast wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        public async Task<RequestResult<Broadcast>> UpdateBroadcast(Broadcast broadcast, CancellationToken ct)
        {
            return await broadcastService.UpdateBroadcastAsync(broadcast, ct);
        }

        /// <summary>
        /// Deletes a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The deleted broadcast wrapped in a RequestResult.</returns>
        /// <response code="200">Returns the deleted broadcast wrapped in a RequestResult.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        public async Task<RequestResult<Broadcast>> DeleteBroadcast(Broadcast broadcast, CancellationToken ct)
        {
            return await broadcastService.DeleteBroadcastAsync(broadcast, ct);
        }
    }
}
