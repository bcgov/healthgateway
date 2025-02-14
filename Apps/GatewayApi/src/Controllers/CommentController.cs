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
namespace HealthGateway.GatewayApi.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle patient notes interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("UserProfile")]
    [ApiController]
    public class CommentController
    {
        private readonly ICommentService commentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentController"/> class.
        /// </summary>
        /// <param name="commentService">The injected comment service.</param>
        public CommentController(
            ICommentService commentService)
        {
            this.commentService = commentService;
        }

        /// <summary>
        /// Posts a UserComment json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="comment">The Comment request model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The comment record was saved.</response>
        /// <response code="400">The request is bad.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("{hdid}/[controller]")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserComment>>> Create(string hdid, [FromBody] UserComment? comment, CancellationToken ct)
        {
            if (comment == null)
            {
                return new BadRequestResult();
            }

            comment.UserProfileId = hdid;
            comment.CreatedBy = hdid;
            comment.UpdatedBy = hdid;
            return await this.commentService.AddAsync(comment, ct);
        }

        /// <summary>
        /// Puts a UserComment json to be updated in the database.
        /// </summary>
        /// <returns>The updated Comment wrapped in a RequestResult.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="comment">The Comment to be updated.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The comment was saved.</response>
        /// <response code="400">The request is bad.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("{hdid}/[controller]")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserComment>>> Update(string hdid, [FromBody] UserComment? comment, CancellationToken ct)
        {
            if (comment == null)
            {
                return new BadRequestResult();
            }

            if (comment.UserProfileId != hdid)
            {
                return new ForbidResult();
            }

            comment.UpdatedBy = hdid;
            return await this.commentService.UpdateAsync(comment, ct);
        }

        /// <summary>
        /// Deletes a UserComment from the database.
        /// </summary>
        /// <returns>The deleted UserComment wrapped in a RequestResult.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="comment">The comment to be deleted.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The note was deleted.</response>
        /// <response code="400">The request is bad.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        [Route("{hdid}/[controller]")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<ActionResult<RequestResult<UserComment>>> Delete(string hdid, [FromBody] UserComment comment, CancellationToken ct)
        {
            return comment.UserProfileId != hdid
                ? new ForbidResult()
                : await this.commentService.DeleteAsync(comment, ct);
        }

        /// <summary>
        /// Gets all comments for the authorized user and event id.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of comments wrapped in a request result.</returns>
        /// <response code="200">Returns the list of comments.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}/[controller]/Entry")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public async Task<RequestResult<IEnumerable<UserComment>>> GetAllForEntry(string hdid, [FromQuery] string parentEntryId, CancellationToken ct)
        {
            return await this.commentService.GetEntryCommentsAsync(hdid, parentEntryId, ct);
        }

        /// <summary>
        /// Gets all comments for the authorized user.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of comments wrapped in a request result.</returns>
        /// <response code="200">Returns the list of comments.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("{hdid}/[controller]")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        [ExcludeFromCodeCoverage]
        public async Task<RequestResult<IDictionary<string, IEnumerable<UserComment>>>> GetAll(string hdid, CancellationToken ct)
        {
            return await this.commentService.GetProfileCommentsAsync(hdid, ct);
        }
    }
}
