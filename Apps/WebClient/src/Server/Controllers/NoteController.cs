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
    using System.Collections.Generic;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Filters;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="noteService">The injected patient notes service.</param>
        public NoteController(
            INoteService noteService)
        {
            this.noteService = noteService;
        }

        /// <summary>
        /// Posts a patient note json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="note">The patient note request model.</param>
        /// <response code="200">The note record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public IActionResult CreateNote(string hdid, [FromBody] UserNote note)
        {
            note.HdId = hdid;
            note.CreatedBy = hdid;
            note.UpdatedBy = hdid;
            RequestResult<UserNote> result = this.noteService.CreateNote(note);
            return new JsonResult(result);
        }

        /// <summary>
        /// Puts a patient note json to be updated in the database.
        /// </summary>
        /// <returns>The updated Note wrapped in a RequestResult.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="note">The patient note.</param>
        /// <response code="200">The note was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public IActionResult UpdateNote(string hdid, [FromBody] UserNote note)
        {
            note.UpdatedBy = hdid;
            RequestResult<UserNote> result = this.noteService.UpdateNote(note);
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
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public IActionResult DeleteNote([FromBody] UserNote note)
        {
            RequestResult<UserNote> result = this.noteService.DeleteNote(note);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets all notes for the specified user.
        /// </summary>
        /// <returns>The list of notes model wrapped in a request result.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <response code="200">Returns the list of notes.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public IActionResult GetAll(string hdid)
        {
            RequestResult<IEnumerable<UserNote>> result = this.noteService.GetNotes(hdid);
            return new JsonResult(result);
        }
    }
}
