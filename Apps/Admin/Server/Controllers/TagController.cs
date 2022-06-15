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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user admin tags.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class TagController
    {
        private readonly IUserFeedbackService feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagController"/> class.
        /// </summary>
        /// <param name="feedbackService">The injected user feedback service.</param>
        public TagController(IUserFeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }

        /// <summary>
        /// Gets all admin tags.
        /// </summary>
        /// <returns>The list of dependent model wrapped in a request result.</returns>
        /// <response code="200">Returns the list of admin tags.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        [Route("[controller]")]
        public RequestResult<IList<AdminTagView>> GetAll()
        {
            RequestResult<IList<AdminTagView>> result = this.feedbackService.GetAllTags();
            return result;
        }

        /// <summary>
        /// Creates a new admin tag.
        /// </summary>
        /// <returns>The newly created tag model.</returns>
        /// <param name="tagName">The tag name.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("[controller]")]
        public RequestResult<AdminTagView> CreateTag([FromBody] string tagName)
        {
            RequestResult<AdminTagView> result = this.feedbackService.CreateTag(tagName);
            return result;
        }

        /// <summary>
        /// Deletes an admin tag.
        /// </summary>
        /// <returns>The deleted tag wrapped in a request result.</returns>
        /// <param name="tag">The admin tag.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Route("[controller]")]
        public RequestResult<AdminTagView> DeleteTag([FromBody] AdminTagView tag)
        {
            RequestResult<AdminTagView> result = this.feedbackService.DeleteTag(tag);
            return result;
        }

        /// <summary>
        /// Associate an existing admin tag to the feedback with matching id.
        /// </summary>
        /// <returns>Returns the user feedback view with the associated admin tag(s) wrapped in a request result.</returns>
        /// <param name="feedbackId">The feedback id.</param>
        /// <param name="tagIds">A list of admin tag ids.</param>
        /// <response code="200">The user feedback containing the added tag model wrapped in a request result.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("UserFeedback/{feedbackId}/[controller]")]
        public RequestResult<UserFeedbackView> AssociateTag(string feedbackId, [FromBody] IList<Guid> tagIds)
        {
            RequestResult<UserFeedbackView> result = this.feedbackService.AssociateFeedbackTag(Guid.Parse(feedbackId), tagIds);
            return result;
        }

        /// <summary>
        /// Dissociates the tag from the given feedback id.
        /// </summary>
        /// <returns>True if the operation was successful.</returns>
        /// <param name="feedbackId">The feedback id.</param>
        /// <param name="tag">The tag model.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpDelete]
        [Route("UserFeedback/{feedbackId}/[controller]")]
        public bool DissociateTag(string feedbackId, [FromBody] UserFeedbackTagView tag)
        {
            bool result = this.feedbackService.DissociateFeedbackTag(Guid.Parse(feedbackId), tag);
            return result;
        }
    }
}
