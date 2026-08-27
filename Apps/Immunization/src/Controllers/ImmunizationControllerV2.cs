//-------------------------------------------------------------------------
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
namespace HealthGateway.Immunization.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Immunization controller for V2.
    /// </summary>
    /// <param name="immunizationService">The immunization data service.</param>
    [Authorize]
    [ApiVersion("2.0")]
    [Route("Immunization")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ImmunizationControllerV2(IImmunizationServiceV2 immunizationService) : ControllerBase
    {
        /// <summary>
        /// Gets immunization records and recommendations.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Immunization records and recommendations for the given patient identifier.</returns>
        /// <response code="200">Returns the immunization result.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Authorize(Policy = ImmunizationPolicy.Read)]
        [ProducesResponseType<ImmunizationResultV2>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ProblemDetails))]
        public async Task<ImmunizationResultV2> GetImmunizations([FromQuery] string hdid, CancellationToken ct)
        {
            return await immunizationService.GetImmunizationsAsync(hdid, ct);
        }
    }
}
