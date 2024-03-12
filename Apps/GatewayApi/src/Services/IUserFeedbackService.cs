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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// The User Feedback service.
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Saves the user feedback to the database.
        /// </summary>
        /// <param name="feedback">The feedback to be saved.</param>
        /// <param name="hdid">The HDID of the person submitting the feedback.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped user feedback.</returns>
        Task<DbResult<UserFeedback>> CreateUserFeedbackAsync(Feedback feedback, string hdid, CancellationToken ct = default);

        /// <summary>
        /// Saves the rating to the database.
        /// </summary>
        /// <param name="rating">The rating model to be saved.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped rating.</returns>
        Task<RequestResult<Rating>> CreateRatingAsync(Rating rating, CancellationToken ct = default);
    }
}
