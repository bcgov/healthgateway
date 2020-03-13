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
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Models;
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
        private readonly INoteService noteService;

        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="noteService">The injected patient notes service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        /// <param name="authorizationService">The injected authorization service.</param>
        public NoteController(
            INoteService noteService,
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
        /// <param name="note">The patient note request model.</param>
        /// <response code="200">The note record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
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

            note.HdId = userHdid;
            note.CreatedBy = userHdid;
            RequestResult<Database.Models.Note> result = this.noteService.CreateNote(note);
            return new JsonResult(result);
        }

        /// <summary>
        /// Puts a patient note json to be updated in the database.
        /// </summary>
        /// <returns>The updated Note wrapped in a RequestResult.</returns>
        /// <param name="note">The patient note.</param>
        /// <response code="200">The note was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> UpdateNote([FromBody] Note note)
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

            note.UpdatedBy = userHdid;
            RequestResult<Note> result = this.noteService.UpdateNote(note);
            return new JsonResult(result);
        }

        /// <summary>
        /// Deletes a note from the database.
        /// </summary>
        /// <returns>The deleted Note wrapped in a RequestResult.</returns>
        /// <param name="note">The patient note.</param>
        /// <response code="200">The note was deleted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> DeleteNote([FromBody] Note note)
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

            RequestResult<Note> result = this.noteService.DeleteNote(note);
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
    }
}
