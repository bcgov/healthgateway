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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to manage beta features available to users.
    /// </summary>
    /// <param name="betaFeatureService">The injected beta feature service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class BetaFeatureController(IBetaFeatureService betaFeatureService) : ControllerBase
    {
        /// <summary>
        /// Sets access to beta features for users with the provided email address.
        /// </summary>
        /// <param name="access">Request model consisting of an email address and collection of available beta features.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">Beta access has been updated.</response>
        /// <response code="404">No user profiles could be found matching the provided email address.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpPut]
        [Route("UserAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task SetUserAccess([FromBody] UserBetaAccess access, CancellationToken ct)
        {
            await betaFeatureService.SetUserAccessAsync(access, ct);
        }

        /// <summary>
        /// Retrieves the beta features available for users with the provided email address.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing the beta features available for users with the provided email address.</returns>
        /// <response code="200">Returns a model containing the beta features available for users with the provided email address.</response>
        /// <response code="404">No user profiles could be found matching the provided email address.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpGet]
        [Route("UserAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<UserBetaAccess> GetUserAccess([FromQuery] string email, CancellationToken ct)
        {
            return await betaFeatureService.GetUserAccessAsync(email, ct);
        }

        /// <summary>
        /// Retrieves a collection containing the emails of all users with beta access and the beta features associated with them.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection containing the emails of all users with beta access and the beta features associated with them.</returns>
        /// <response code="200">
        /// Returns a collection containing the emails of all users with beta access and the beta features
        /// associated with them.
        /// </response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpGet]
        [Route("AllUserAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<UserBetaAccess>> GetAllUserAccess(CancellationToken ct)
        {
            return await betaFeatureService.GetAllUserAccessAsync(ct);
        }
    }
}
