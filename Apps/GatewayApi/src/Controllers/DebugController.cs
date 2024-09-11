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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace HealthGateway.GatewayApi.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle system communications.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        /// <summary>
        /// Returns an empty HTTP 200 response.
        /// </summary>
        /// <returns>An empty response body.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">Expected return value.</response>
        [HttpGet]
        [Route("200")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get200(CancellationToken ct)
        {
            return new OkResult();
        }

        /// <summary>
        /// Returns an HTTP 400 validation problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="400">Expected return value.</response>
        [HttpGet]
        [Route("400")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get400(CancellationToken ct)
        {
            throw new ValidationException("Sample validation failure", [new("MysteryParameter", "Mystery parameter was not supplied.")]);
        }

        /// <summary>
        /// Returns an HTTP 401 problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="401">Expected return value.</response>
        [HttpGet]
        [Route("401")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get401(CancellationToken ct)
        {
            return new UnauthorizedResult();
        }

        /// <summary>
        /// Returns an HTTP 403 problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="403">Expected return value.</response>
        [HttpGet]
        [Route("403")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get403(CancellationToken ct)
        {
            return new ForbidResult();
        }

        /// <summary>
        /// Returns an HTTP 409 problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="409">Expected return value.</response>
        [HttpGet]
        [Route("409")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get409(CancellationToken ct)
        {
            throw new AlreadyExistsException();
        }

        /// <summary>
        /// Returns an HTTP 500 problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="500">Expected return value.</response>
        [HttpGet]
        [Route("500")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get500(CancellationToken ct)
        {
            throw new DatabaseException();
        }

        /// <summary>
        /// Returns an HTTP 502 problem details response.
        /// </summary>
        /// <returns>A problem details response.</returns>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="502">Expected return value.</response>
        [HttpGet]
        [Route("502")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ProblemDetails))]
        [ApiExplorerSettings(GroupName = "debug")]
        public async Task<IActionResult> Get502(CancellationToken ct)
        {
            throw new UpstreamServiceException();
        }
    }
}
