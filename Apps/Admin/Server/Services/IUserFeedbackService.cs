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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that provides admin functionality to User feedback.
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Retrieves the user feedback.
        /// </summary>
        /// <returns>returns a list of user feedbacks wrapped in a request result.</returns>
        RequestResult<IList<UserFeedbackView>> GetUserFeedback();

        /// <summary>
        /// Updates the user feedback.
        /// </summary>
        /// <param name="feedback">The user feedback to update.</param>
        /// <returns>True if the call was success.</returns>
        bool UpdateFeedbackReview(UserFeedbackView feedback);

        /// <summary>
        /// Retrieves the admin tags.
        /// </summary>
        /// <returns>returns a list of admin tags wrapped in a request result.</returns>
        RequestResult<IList<AdminTagView>> GetAllAdminTags();

        /// <summary>
        /// Creates a new feedback tag and associates it with the user feedback id.
        /// </summary>
        /// <param name="userFeedbackId">The user feedback id to be associated to the new tag.</param>
        /// <param name="tagName">The tag name.</param>
        /// <returns>returns the newly created admin tag wrapped in a request result.</returns>
        RequestResult<UserFeedbackTagView> CreateFeedbackTag(Guid userFeedbackId, string tagName);

        /// <summary>
        /// Associates an admin tag to a feedback.
        /// </summary>
        /// <param name="userFeedbackId">The user feedback id to be associated to the tag.</param>
        /// <param name="tag">The admin tag.</param>
        /// <returns>returns the associated admin tag wrapped in a request result.</returns>
        RequestResult<UserFeedbackTagView> AssociateFeedbackTag(Guid userFeedbackId, AdminTagView tag);

        /// <summary>
        /// Dissociates an admin tag from a user feedback.
        /// </summary>
        /// <param name="userFeedbackId">The user feedback id to be dissociated to the tag.</param>
        /// <param name="tag">The admin tag.</param>
        /// <returns>returns the associated admin tag wrapped in a request result.</returns>
        bool DissociateFeedbackTag(Guid userFeedbackId, UserFeedbackTagView tag);
    }
}
