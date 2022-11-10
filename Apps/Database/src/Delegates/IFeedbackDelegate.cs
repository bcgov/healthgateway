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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the UserFeedback model.
    /// </summary>
    public interface IFeedbackDelegate
    {
        /// <summary>
        /// Creates a UserFeedback object in the database.
        /// </summary>
        /// <param name="feedback">The feedback to create.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<UserFeedback> InsertUserFeedback(UserFeedback feedback);

        /// <summary>
        /// Updates the UserFeedback object in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will overridden by our framework.
        /// </summary>
        /// <param name="feedback">The feedback to update.</param>
        void UpdateUserFeedback(UserFeedback feedback);

        /// <summary>
        /// Updates the UserFeedback object including user feedback tag associations in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will overridden by our framework.
        /// </summary>
        /// <param name="feedback">The feedback to update.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<UserFeedback> UpdateUserFeedbackWithTagAssociations(UserFeedback feedback);

        /// <summary>
        /// Fetches the UserFeedback from the database.
        /// </summary>
        /// <param name="feedbackId">The unique feedback id to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<UserFeedback> GetUserFeedback(Guid feedbackId);

        /// <summary>
        /// Fetches the UserFeedback with FeedbackTag associations from the database.
        /// </summary>
        /// <param name="feedbackId">The unique feedback id to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<UserFeedback> GetUserFeedbackWithFeedbackTags(Guid feedbackId);

        /// <summary>
        /// Fetches the UserFeedback with FeedbackTag associations from the database.
        /// </summary>
        /// <returns>A DB result which encapsulates the return objects and status.</returns>
        DbResult<IList<UserFeedback>> GetAllUserFeedbackEntries();
    }
}
