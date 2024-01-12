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
namespace HealthGateway.Admin.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user admin tags.
    /// </summary>
    /// <param name="feedbackService">The injected user feedback service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class TagController(IUserFeedbackService feedbackService)
    {
        /// <summary>
        /// Gets all admin tags.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of admin tags wrapped in a request result.</returns>
        /// <response code="200">Returns the list of admin tags.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        [Route("[controller]")]
        public async Task<RequestResult<IList<AdminTagView>>> GetAll(CancellationToken ct)
        {
            RequestResult<IList<AdminTagView>> result = await feedbackService.GetAllTagsAsync(ct);
            return result;
        }

        /// <summary>
        /// Creates a new admin tag.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The newly created tag model.</returns>
        /// <response code="200">Returns the newly created tag model.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("[controller]")]
        public async Task<RequestResult<AdminTagView>> CreateTag([FromBody] string tagName, CancellationToken ct)
        {
            RequestResult<AdminTagView> result = await feedbackService.CreateTagAsync(tagName, ct);
            return result;
        }

        /// <summary>
        /// Deletes an admin tag.
        /// </summary>
        /// <param name="tag">The admin tag.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The deleted tag wrapped in a request result.</returns>
        /// <response code="200">Returns the deleted tag wrapped in a request result.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpDelete]
        [Route("[controller]")]
        public async Task<RequestResult<AdminTagView>> DeleteTag([FromBody] AdminTagView tag, CancellationToken ct)
        {
            RequestResult<AdminTagView> result = await feedbackService.DeleteTagAsync(tag, ct);
            return result;
        }

        /// <summary>
        /// Associate a collection of existing admin tags to the feedback with matching id.
        /// </summary>
        /// <param name="feedbackId">The feedback id.</param>
        /// <param name="tagIds">A list of admin tag ids.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Returns the user feedback view with the associated admin tag(s) wrapped in a request result.</returns>
        /// <response code="200">The user feedback containing the added tag model wrapped in a request result.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPut]
        [Route("UserFeedback/{feedbackId}/[controller]")]
        public async Task<RequestResult<UserFeedbackView>> AssociateTags(Guid feedbackId, [FromBody] IList<Guid> tagIds, CancellationToken ct)
        {
            RequestResult<UserFeedbackView> result = await feedbackService.AssociateFeedbackTagsAsync(feedbackId, tagIds, ct);
            return result;
        }
    }
}
