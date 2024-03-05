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
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback review.
    /// </summary>
    /// <param name="feedbackService">The injected user feedback service.</param>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser,AdminReviewer")]
    public class UserFeedbackController(IUserFeedbackService feedbackService)
    {
        /// <summary>
        /// Retrieves a list of user feedbacks.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of user feedbacks.</returns>
        /// <response code="200">Returns the list of user feedbacks.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpGet]
        public async Task<RequestResult<IList<UserFeedbackView>>> GetFeedbackList(CancellationToken ct)
        {
            return await feedbackService.GetUserFeedbackAsync(ct);
        }

        /// <summary>
        /// Updates a user feedback.
        /// </summary>
        /// <param name="feedback">The user feedback to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated user feedback view wrapped in a request result.</returns>
        /// <response code="200">Returns the updated user feedback view wrapped in a request result.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPatch]
        public async Task<RequestResult<UserFeedbackView>> UpdateUserFeedback(UserFeedbackView feedback, CancellationToken ct)
        {
            return await feedbackService.UpdateFeedbackReviewAsync(feedback, ct);
        }
    }
}
