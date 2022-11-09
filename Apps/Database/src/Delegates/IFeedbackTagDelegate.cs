//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for UserFeedbackTag.
    /// </summary>
    public interface IFeedbackTagDelegate
    {
        /// <summary>
        /// Add the given user feedback tag.
        /// </summary>
        /// <param name="feedbackTag">The user feedback tag to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A user feedback tag wrapped in a DBResult.</returns>
        DbResult<UserFeedbackTag> Add(UserFeedbackTag feedbackTag, bool commit = true);

        /// <summary>
        /// Deletes the given user feedback tag.
        /// </summary>
        /// <param name="feedbackTag">The user feedback tag to be deleted from database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>The result DBResult.</returns>
        DbResult<UserFeedbackTag> Delete(UserFeedbackTag feedbackTag, bool commit = true);

        /// <summary>
        /// Gets a list of user feedback tags by feedback id.
        /// </summary>
        /// <param name="feedbackId">The feedback id to search on.</param>
        /// <returns>An IEnumerable of UserFeedbackTag wrapped in a DBResult.</returns>
        DbResult<IEnumerable<UserFeedbackTag>> GetUserFeedbackTagsByFeedbackId(Guid feedbackId);
    }
}
