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
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Models;
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
    public class CommentController
    {
        private readonly ICommentService commentService;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentController"/> class.
        /// </summary>
        /// <param name="commentService">The injected comment service.</param>
        /// <param name="httpContextAccessor">The injected http context accessor provider.</param>
        public CommentController(
            ICommentService commentService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.commentService = commentService;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Posts a UserComment json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="comment">The Comment request model.</param>
        /// <response code="200">The comment record was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Authorize(Policy = UserPolicy.UserOnly)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult Create([FromHeader] string authorization, [FromBody] UserComment comment)
        {
            // Validate the hdid to be a patient.
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;

            if (comment == null)
            {
                return new BadRequestResult();
            }

            comment.UserProfileId = userHdid;
            comment.CreatedBy = userHdid;
            comment.UpdatedBy = userHdid;
            RequestResult<UserComment> result = this.commentService.Add(comment);
            return new JsonResult(result);
        }

        /// <summary>
        /// Puts a UserComment json to be updated in the database.
        /// </summary>
        /// <returns>The updated Comment wrapped in a RequestResult.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="comment">The Comment to be updated.</param>
        /// <response code="200">The comment was saved.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Authorize(Policy = UserPolicy.UserOnly)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult Update([FromHeader] string authorization, [FromBody] UserComment comment)
        {
            if (comment == null)
            {
                return new BadRequestResult();
            }

            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            if (comment.UserProfileId != userHdid)
            {
                return new ForbidResult();
            }

            comment.UpdatedBy = userHdid;
            RequestResult<UserComment> result = this.commentService.Update(comment);
            return new JsonResult(result);
        }

        /// <summary>
        /// Deletes a UserComment from the database.
        /// </summary>
        /// <returns>The deleted UserComment wrapped in a RequestResult.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="comment">The comment to be deleted.</param>
        /// <response code="200">The note was deleted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Authorize(Policy = UserPolicy.UserOnly)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult Delete([FromHeader] string authorization, [FromBody] UserComment comment)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            if (comment.UserProfileId != userHdid)
            {
                return new ForbidResult();
            }

            RequestResult<UserComment> result = this.commentService.Delete(comment);
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets all comments for the authorized user and event id.
        /// </summary>
        /// <returns>The list of comments wrapped in a request result.</returns>
        /// <param name="authorization">The bearer token of the authenticated user.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <response code="200">Returns the list of comments.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Authorize(Policy = UserPolicy.UserOnly)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Used by Swagger to display input for authorization header.")]
        public IActionResult GetAllForEntry([FromHeader] string authorization, [FromQuery] string parentEntryId)
        {
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            string userHdid = user.FindFirst("hdid").Value;
            RequestResult<IEnumerable<UserComment>> result = this.commentService.GetList(userHdid, parentEntryId);
            return new JsonResult(result);
        }
    }
}
