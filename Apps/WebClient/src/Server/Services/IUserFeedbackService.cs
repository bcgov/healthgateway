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
namespace HealthGateway.WebClient.Services
{
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// The User Feedback service.
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Saves the user feedback to the database.
        /// </summary>
        /// <param name="userFeedback">The user feedback model to be saved.</param>
        /// <returns>The wrapped user feedback.</returns>
        DBResult<UserFeedback> CreateUserFeedback(UserFeedback userFeedback);

        /// <summary>
        /// Saves the rating to the database.
        /// </summary>
        /// <param name="rating">The rating model to be saved.</param>
        /// <returns>The wrapped rating.</returns>
        RequestResult<Rating> CreateRating(Rating rating);
    }
}
