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
namespace HealthGateway.Admin.Services
{
    using System.Collections.Generic;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service that provides functionality to access and create requests for beta access.
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Retrieves the beta requests where the user does not have an invite yet.
        /// </summary>
        /// <returns>returns the beta request for the user if found.</returns>
        RequestResult<List<UserFeedbackView>> GetUserFeedback();

        /// <summary>
        /// Updates the user feedback.
        /// </summary>
        /// <param name="feedback">The user feedback to update.</param>
        /// <returns>True if the call was success.</returns>
        bool UpdateFeedbackReview(UserFeedbackView feedback);
    }
}