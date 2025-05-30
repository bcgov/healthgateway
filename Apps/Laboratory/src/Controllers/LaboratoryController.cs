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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Filters;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The authenticated laboratory controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("SonarLint", "S6960:This controller has multiple responsibilities and could be split into 2 smaller controllers", Justification = "Team decision")]
    public class LaboratoryController : ControllerBase
    {
        private readonly ILaboratoryService labService;
        private readonly ILabTestKitService labTestKitService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryController"/> class.
        /// </summary>
        /// <param name="labService">The laboratory data service.</param>
        /// <param name="labTestKitService">The lab testkit service to use.</param>
        public LaboratoryController(ILaboratoryService labService, ILabTestKitService labTestKitService)
        {
            this.labService = labService;
            this.labTestKitService = labTestKitService;
        }

        /// <summary>
        /// Gets a result containing a collection of COVID-19 laboratory orders.
        /// </summary>
        /// <param name="hdid">The hdid resource to request the COVID-19 laboratory orders for.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// Returns collection of COVID-19 laboratory orders if available and information about whether the orders could
        /// be retrieved.
        /// </returns>
        /// <response code="200">Returns the result model.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("Covid19Orders")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<RequestResult<Covid19OrderResult>> GetCovid19Orders([FromQuery] string hdid, CancellationToken ct)
        {
            return await this.labService.GetCovid19OrdersAsync(hdid, ct: ct);
        }

        /// <summary>
        /// Gets a result containing a collection of laboratory orders.
        /// </summary>
        /// <param name="hdid">The hdid resource to request the laboratory orders for.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// Returns collection of laboratory orders if available and information about whether the orders could be
        /// retrieved.
        /// </returns>
        /// <response code="200">Returns the result model.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("LaboratoryOrders")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<RequestResult<LaboratoryOrderResult>> GetLaboratoryOrders([FromQuery] string hdid, CancellationToken ct)
        {
            return await this.labService.GetLaboratoryOrdersAsync(hdid, ct);
        }

        /// <summary>
        /// Gets a specific laboratory report.
        /// </summary>
        /// <param name="reportId">The ID of the report belonging to the authenticated user to fetch.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="isCovid19">Indicates whether the COVID-19 report should be returned.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A laboratory PDF report wrapped in a request result.</returns>
        /// <response code="200">Returns the specified PDF lab report.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{reportId}/Report")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<RequestResult<LaboratoryReport>> GetLaboratoryReport(string reportId, [FromQuery] string hdid, bool isCovid19 = true, CancellationToken ct = default)
        {
            Activity.Current?.AddBaggage("LaboratoryReportId", reportId);
            Activity.Current?.AddBaggage("LaboratoryReportIsCovid19", isCovid19.ToString());
            return await this.labService.GetLabReportAsync(reportId, hdid, isCovid19, ct);
        }

        /// <summary>
        /// Registers a lab test for an authenticated user.
        /// </summary>
        /// <param name="hdid">The hdid to apply the LabTestKit against.</param>
        /// <param name="labTestKit">The labTestKit to register.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A LabTestKit  Result object wrapped in a request result.</returns>
        /// <response code="200">The LabTestKit was processed.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpPost]
        [Produces("application/json")]
        [Route("{hdid}/LabTestKit")]
        [Authorize(Policy = LaboratoryPolicy.Write)]
        public async Task<RequestResult<LabTestKit>> AddLabTestKit(string hdid, [FromBody] LabTestKit labTestKit, CancellationToken ct)
        {
            Activity.Current?.AddBaggage("TestKitId", labTestKit.TestKitId);
            return await this.labTestKitService.RegisterLabTestKitAsync(hdid, labTestKit, ct);
        }
    }
}
