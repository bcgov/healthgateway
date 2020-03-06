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
namespace HealthGateway.WebClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle patient notes interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class NoteController
    {
        private readonly NoteService noteService;

        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="noteService">The injected patient notes service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        public NoteController(
            NoteService noteService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            this.noteService = noteService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorizationService = authorizationService;
        }

        /// <summary>
        /// Posts a patient note json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="model">The patient note request model.</param>
        /// <response code="200">The note record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> CreateNote([FromBody] Database.Models.Note model)
        {
            // Validate the hdid to be a patient.
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            AuthorizationResult isAuthorized = await this.authorizationService
                .AuthorizeAsync(user, userHdid, PolicyNameConstants.UserIsPatient)
                .ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            // Validate that the jwt hdid matches the post body
            if (model == null ||
                !userHdid.Equals(model!.HdId, StringComparison.CurrentCultureIgnoreCase))
            {
                return new BadRequestResult();
            }

            RequestResult<Database.Models.Note> result = this.noteService.CreateNote(model);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets all notes for the specified user.
        /// </summary>
        /// <returns>The list of notes model wrapped in a request result.</returns>
        /// <response code="200">Returns the list of notes.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            var isAuthorized = await this.authorizationService
                .AuthorizeAsync(user, userHdid, PolicyNameConstants.UserIsPatient)
                .ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            RequestResult<IEnumerable<Database.Models.Note>> result = this.noteService.GetNotes(userHdid);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a single notes by its id.
        /// </summary>
        /// <returns>The note model wrapped in a request result.</returns>
        /// <param name="noteId">The note id.</param>
        /// <response code="200">Returns the note model.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{noteId}")]
        public async Task<IActionResult> Get(Guid noteId)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            var isAuthorized = await this.authorizationService
                .AuthorizeAsync(user, userHdid, PolicyNameConstants.UserIsPatient)
                .ConfigureAwait(true);
            if (!isAuthorized.Succeeded)
            {
                return new ForbidResult();
            }

            // RequestResult<Database.Models.Note> result = this.noteService.Get(noteId);

            // TODO: Compare note hdid with current jwt hdid.
            // TODO: Do we need to implement this?
            return new JsonResult(new object());
        }
    }
}
