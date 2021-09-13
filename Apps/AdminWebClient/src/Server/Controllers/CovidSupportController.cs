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
namespace HealthGateway.Admin.Controllers
{
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Support;
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle Covid support requests.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "SupportUser")]
    public class CovidSupportController
    {
        private readonly ICovidSupportService covidSupportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidSupportController"/> class.
        /// </summary>
        /// <param name="covidSupportService">The injected covid support service.</param>
        public CovidSupportController(ICovidSupportService covidSupportService)
        {
            this.covidSupportService = covidSupportService;
        }

        /// <summary>
        /// Retrieves the patient's covid information for the given identifier.
        /// </summary>
        /// <returns>The covid information for the given phn identifier.</returns>
        /// <param name="phn">The personal health number that matches the person to retrieve.</param>
        /// <response code="200">Returns the wrapped result of the request.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("Patient")]
        public async Task<IActionResult> GetPatient([FromHeader] string phn)
        {
            return new JsonResult(await this.covidSupportService.GetCovidInformation(phn).ConfigureAwait(true));
        }

        /// <summary>
        /// Triggers the process to physically mail the Vaccine Card document.
        /// </summary>
        /// <returns>A wrapped result indicating the mail status.</returns>
        /// <param name="request">The mail document request.</param>
        /// <response code="200">Returns the wrapped result of the request.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("Patient/Document")]
        public async Task<IActionResult> MailVaccineCard([FromBody] MailDocumentRequest request)
        {
            return new JsonResult(await this.covidSupportService.MailVaccineCardAsync(request).ConfigureAwait(true));
        }

        /// <summary>
        /// Gets the COVID-19 Vaccine Record document that includes the Vaccine Card and Vaccination History.
        /// </summary>
        /// <returns>The encoded immunization document.</returns>
        /// <param name="phn">The personal health number that matches the document to retrieve.</param>
        /// <response code="200">Returns the wrapped result of the request.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("Patient/Document")]
        public async Task<IActionResult> RetrieveVaccineRecord([FromHeader] string phn)
        {
            return new JsonResult(await this.covidSupportService.RetrieveVaccineRecordAsync(phn).ConfigureAwait(true));
        }
    }
}
