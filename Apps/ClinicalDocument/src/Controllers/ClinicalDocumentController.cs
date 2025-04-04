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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Services;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The clinical document controller.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class ClinicalDocumentController : ControllerBase
    {
        private readonly IClinicalDocumentService clinicalDocumentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClinicalDocumentController"/> class.
        /// </summary>
        /// <param name="clinicalDocumentService">Injected clinical document service.</param>
        public ClinicalDocumentController(IClinicalDocumentService clinicalDocumentService)
        {
            this.clinicalDocumentService = clinicalDocumentService;
        }

        /// <summary>
        /// Gets the collection of clinical document records associated with the given HDID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
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
        [Authorize(Policy = ClinicalDocumentPolicy.Read)]
        public async Task<RequestResult<IEnumerable<ClinicalDocumentRecord>>> GetRecords(string hdid, CancellationToken ct)
        {
            return await this.clinicalDocumentService.GetRecordsAsync(hdid, ct);
        }

        /// <summary>
        /// Gets a specific clinical document file associated with the given HDID and file ID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="fileId">The ID of the file to fetch.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
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
        [Authorize(Policy = ClinicalDocumentPolicy.Read)]
        public async Task<RequestResult<EncodedMedia>> GetFile(string hdid, string fileId, CancellationToken ct)
        {
            Activity.Current?.AddBaggage("FileId", fileId);
            return await this.clinicalDocumentService.GetFileAsync(hdid, fileId, ct);
        }
    }
}
