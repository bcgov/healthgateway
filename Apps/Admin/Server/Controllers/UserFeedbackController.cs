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
    using System.Collections.Generic;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback review.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class UserFeedbackController
    {
        private readonly IUserFeedbackService feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackController"/> class.
        /// </summary>
        /// <param name="feedbackService">The injected user feedback service.</param>
        public UserFeedbackController(IUserFeedbackService feedbackService)
        {
            this.feedbackService = feedbackService;
        }

        /// <summary>
        /// Retrieves a list of user feedbacks.
        /// </summary>
        /// <returns>The list of user feedbacks.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpGet]
        public RequestResult<IList<UserFeedbackView>> GetFeedbackList()
        {
            return this.feedbackService.GetUserFeedback();
        }

        /// <summary>
        /// Sends email invites to the beta requests with the given ids.
        /// </summary>
        /// <returns>A list of ids of the beta requests that where successfully processed.</returns>
        /// <param name="feedback">user feedback to update.</param>
        /// <response code="200">Returns the beta requests ids that where invited.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPatch]
        public RequestResult<UserFeedbackView> UpdateUserFeedback(UserFeedbackView feedback)
        {
            return this.feedbackService.UpdateFeedbackReview(feedback);
        }
    }
}
