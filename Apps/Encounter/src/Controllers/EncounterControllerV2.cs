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
namespace HealthGateway.Encounter.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The encounter controller.
    /// </summary>
    /// <param name="encounterService">The encounter data service.</param>
    [Authorize]
    [ApiVersion("2.0")]
    [Route("Encounter")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class EncounterControllerV2(IEncounterServiceV2 encounterService) : ControllerBase
    {
        /// <summary>
        /// Gets a json list of hospital visit records.
        /// </summary>
        /// <param name="hdid">The hdid patient id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of hospital visit records.</returns>
        /// <response code="200">Returns the list of hospital visit records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("HospitalVisit/{hdid}")]
        [Authorize(Policy = EncounterPolicy.Read)]
        public async Task<IReadOnlyList<HospitalVisitModel>> GetHospitalVisits(string hdid, CancellationToken ct)
        {
            return await encounterService.GetHospitalVisitsAsync(hdid, ct);
        }
    }
}
