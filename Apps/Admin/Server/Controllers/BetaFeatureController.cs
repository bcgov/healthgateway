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
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to manage beta feature access to the admin website.
    /// </summary>
    /// <param name="betaFeatureAccessService">The injected beta feature access service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class BetaFeatureController(IBetaFeatureAccessService betaFeatureAccessService) : ControllerBase
    {
        /// <summary>
        /// Sets access to beta features for profile associated with the provided email.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        /// <param name="request">The request object containing beta features to set.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">Beta feature access has been updated.</response>
        /// <response code="404">User profile with provided email not found.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpPut]
        [Route("{email}/UserAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "AdminUser")]
        public async Task SetUserAccess(string email, BetaFeatureAccessRequest request, CancellationToken ct)
        {
            await betaFeatureAccessService.SetUserAccessAsync(email, request.BetaFeatures, ct);
        }

        /// <summary>
        /// Gets a list of beta features available for the profile associated with the provided email.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">Beta feature access has been updated.</response>
        /// <response code="404">User profile with provided email not found.</response>
        /// <response code="401">The client must authenticate itself to get the requested resource.</response>
        [HttpGet]
        [Route("/UserAccess")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = "AdminUser")]
        public async Task<IEnumerable<BetaFeature>> GetUserAccess(string email, CancellationToken ct)
        {
            return await betaFeatureAccessService.GetUserAccessAsync(email, ct);
        }
    }
}
