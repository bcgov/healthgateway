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
namespace HealthGateway.Laboratory.Controllers
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The public laboratory controller.
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class PublicLaboratoryController : ControllerBase
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicLaboratoryController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        public PublicLaboratoryController(
            ILogger<PublicLaboratoryController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Requests the COVID-19 test results for the supplied PHN, date of birth, and collection date.
        /// </summary>
        /// <param name="phn">The Personal Health Number to query.</param>
        /// <param name="dateOfBirth">The date of birth (yyyy-MM-dd) associated with the supplied PHN.</param>
        /// <param name="collectionDate">The date the test was collected (yyyy-MM-dd).</param>
        /// <returns>The wrapped collection of COVID-19 test results or an appropriate error.</returns>
        /// <response code="200">Returns the collection of COVID-19 test results or an appropriate error.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Route("CovidTests")]
        [Produces("application/json")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<RequestResult<PublicCovidTestResponseResult>> CovidTests([FromHeader] string phn, [FromHeader] string dateOfBirth, [FromHeader] string collectionDate)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }
    }
}
