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
    using HealthGateway.Admin.Server.Models;
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
        public TagController(
            IUserFeedbackService feedbackService)
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
        public IActionResult GetAll()
        {
            RequestResult<IList<AdminTagView>> result = this.feedbackService.GetAllAdminTags();
            return new JsonResult(result);
        }

        /// <summary>
        /// Creates a new admin tag.
        /// </summary>
        /// <returns>The newly created tag model.</returns>
        /// <param name="feedbackId">The feedback id.</param>
        /// <param name="tagName">The tag name.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        [Route("UserFeedback/{feedbackId}/[controller]")]
        public IActionResult CreateTag(string feedbackId, [FromBody] string tagName)
        {
            RequestResult<UserFeedbackTagView> result = this.feedbackService.CreateFeedbackTag(Guid.Parse(feedbackId), tagName);
            return new JsonResult(result);
        }

        /// <summary>
        /// Associate an existing admin tag to the feedback with matching id.
        /// </summary>
        /// <returns>The added tag model wrapped in a request result.</returns>
        /// <param name="feedbackId">The feedback id.</param>
        /// <param name="tag">The tag model.</param>
        /// <response code="200">Returns the list of dependents.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPut]
        [Route("UserFeedback/{feedbackId}/[controller]")]
        public IActionResult AssociateTag(string feedbackId, [FromBody] AdminTagView tag)
        {
            RequestResult<UserFeedbackTagView> result = this.feedbackService.AssociateFeedbackTag(Guid.Parse(feedbackId), tag);
            return new JsonResult(result);
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
        public IActionResult DissociateTag(string feedbackId, [FromBody] UserFeedbackTagView tag)
        {
            bool result = this.feedbackService.DissociateFeedbackTag(Guid.Parse(feedbackId), tag);
            return new JsonResult(result);
        }
    }
}
