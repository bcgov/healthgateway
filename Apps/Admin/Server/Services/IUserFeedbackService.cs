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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that provides admin functionality to User feedback.
    /// </summary>
    public interface IUserFeedbackService
    {
        /// <summary>
        /// Retrieves the user feedback.
        /// </summary>
        /// <returns>A list of user feedback wrapped in a request result.</returns>
        RequestResult<IList<UserFeedbackView>> GetUserFeedback();

        /// <summary>
        /// Updates the user feedback.
        /// </summary>
        /// <param name="feedback">The user feedback to update.</param>
        /// <returns>Returns the user feedback view with its associated admin tag(s) wrapped in a request result.</returns>
        RequestResult<UserFeedbackView> UpdateFeedbackReview(UserFeedbackView feedback);

        /// <summary>
        /// Retrieves the admin tags.
        /// </summary>
        /// <returns>A list of admin tags wrapped in a request result.</returns>
        RequestResult<IList<AdminTagView>> GetAllTags();

        /// <summary>
        /// Creates a new admin tag.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <returns>The newly created admin tag wrapped in a request result.</returns>
        RequestResult<AdminTagView> CreateTag(string tagName);

        /// <summary>
        /// Deletes an admin tag.
        /// </summary>
        /// <param name="tag">The admin tag.</param>
        /// <returns>The deleted admin tag wrapped in a request result.</returns>
        RequestResult<AdminTagView> DeleteTag(AdminTagView tag);

        /// <summary>
        /// Associates a collection of tags to a feedback item.
        /// </summary>
        /// <param name="userFeedbackId">The user feedback id to be associated to the tags.</param>
        /// <param name="adminTagIds">The admin tag ids.</param>
        /// <returns>Returns the user feedback view with its associated admin tag(s) wrapped in a request result.</returns>
        RequestResult<UserFeedbackView> AssociateFeedbackTags(Guid userFeedbackId, IList<Guid> adminTagIds);
    }
}
