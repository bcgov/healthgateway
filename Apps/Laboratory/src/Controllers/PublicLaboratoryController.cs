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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The public laboratory controller.
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class PublicLaboratoryController : ControllerBase
    {
        /// <summary>
        /// Gets or sets the laboratory data service.
        /// </summary>
        private readonly ILabTestKitService labTestKitService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicLaboratoryController"/> class.
        /// </summary>
        /// <param name="labTestKitService">The lab testkit service to use.</param>
        public PublicLaboratoryController(ILabTestKitService labTestKitService)
        {
            this.labTestKitService = labTestKitService;
        }

        /// <summary>
        /// Registers a lab test for a public user.
        /// </summary>
        /// <param name="labTestKit">The lab test kit to register.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The lab test kit wrapped in a request result.</returns>
        /// <response code="200">The LabTestKit was processed.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpPost]
        [Produces("application/json")]
        [Route("LabTestKit")]
        public async Task<RequestResult<PublicLabTestKit>> AddLabTestKit([FromBody] PublicLabTestKit labTestKit, CancellationToken ct)
        {
            return await this.labTestKitService.RegisterLabTestKitAsync(labTestKit, ct);
        }
    }
}
