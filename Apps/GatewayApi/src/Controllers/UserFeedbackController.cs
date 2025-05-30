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
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback interactions.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class UserFeedbackController
    {
        private readonly IUserFeedbackService userFeedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackController"/> class.
        /// </summary>
        /// <param name="userFeedbackService">The injected user feedback service.</param>
        public UserFeedbackController(
            IUserFeedbackService userFeedbackService)
        {
            this.userFeedbackService = userFeedbackService;
        }

        /// <summary>
        /// Posts a user feedback json to be inserted into the database.
        /// </summary>
        /// <returns>The http status.</returns>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="feedback">The user feedback model.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <response code="200">The user feedback record was saved.</response>
        /// <response code="400">The user feedback object is invalid.</response>
        /// <response code="409">The user feedback was already inserted.</response>
        /// <response code="401">The client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("{hdid}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<IActionResult> CreateUserFeedback(string hdid, [FromBody] Feedback? feedback, CancellationToken ct)
        {
            if (feedback == null)
            {
                return new BadRequestResult();
            }

            DbResult<UserFeedback> result = await this.userFeedbackService.CreateUserFeedbackAsync(feedback, hdid, ct);
            return result.Status != DbStatusCode.Created ? new ConflictResult() : new OkResult();
        }

        /// <summary>
        /// Saves the rating model.
        /// </summary>
        /// <param name="rating">The rating to be saved.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The saved rating wrapped in a request result.</returns>
        /// <response code="200">Returns the saved rating json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">
        /// The client does not have access rights to the content; that is, it is unauthorized, so the server
        /// is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.
        /// </response>
        [HttpPost]
        [Route("Rating")]
        public async Task<RequestResult<RatingModel>> CreateRating(SubmitRating rating, CancellationToken ct)
        {
            return await this.userFeedbackService.CreateRatingAsync(rating, ct);
        }
    }
}
