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
namespace HealthGateway.ClinicalDocument.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Services;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The clinical document controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    [ExcludeFromCodeCoverage]
    public class ClinicalDocumentController : ControllerBase
    {
        private readonly ILogger<ClinicalDocumentController> logger;
        private readonly IClinicalDocumentService clinicalDocumentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClinicalDocumentController"/> class.
        /// </summary>
        /// <param name="logger">Injected logger.</param>
        /// <param name="clinicalDocumentService">Injected service.</param>
        public ClinicalDocumentController(ILogger<ClinicalDocumentController> logger, IClinicalDocumentService clinicalDocumentService)
        {
            this.logger = logger;
            this.clinicalDocumentService = clinicalDocumentService;
        }

        /// <summary>
        /// Gets the collection of clinical document records associated with the given HDID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <returns>The collection of clinical document records.</returns>
        /// <response code="200">Returns the collection of clinical document records.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<RequestResult<IEnumerable<ClinicalDocumentRecord>>> GetRecords([FromQuery] string hdid)
        {
            this.logger.LogDebug("Getting clinical document records for HDID: {Hdid}", hdid);
            RequestResult<IEnumerable<ClinicalDocumentRecord>> result = await this.clinicalDocumentService.GetRecordsAsync(hdid).ConfigureAwait(true);
            this.logger.LogDebug("Finished getting clinical document records for HDID: {Hdid}", hdid);
            return result;
        }

        /// <summary>
        /// Gets a specific clinical document file associated with the given HDID and file ID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="fileId">The ID of the file to fetch.</param>
        /// <returns>The specified clinical document file.</returns>
        /// <response code="200">Returns the specified clinical document file.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        /// <response code="503">The service is unavailable for use.</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("{hdid}/file/{fileId}")]
        [Authorize(Policy = LaboratoryPolicy.Read)]
        public async Task<RequestResult<EncodedMedia>> GetFile(string hdid, string fileId)
        {
            this.logger.LogDebug("Getting clinical document file for Hdid: {Hdid} with file ID: {FileId}", hdid, fileId);
            RequestResult<EncodedMedia> result = await this.clinicalDocumentService.GetFileAsync(hdid, fileId).ConfigureAwait(true);
            this.logger.LogDebug("Finished getting clinical document file for Hdid: {Hdid} with file ID: {FileId}", hdid, fileId);
            return result;
        }
    }
}
