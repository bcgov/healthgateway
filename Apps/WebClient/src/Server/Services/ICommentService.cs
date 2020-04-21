﻿// -------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to interact with the Comment Delegate.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Adds a Comment in the backend.
        /// </summary>
        /// <param name="comment">The Comment to be created.</param>
        /// <returns>A Comment wrapped in a RequestResult.</returns>
        public RequestResult<Comment> Add(Comment comment);

        /// <summary>
        /// Gets a list of Comments for the given hdId and event id.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <returns>A List of Comments wrapped in a RequestResult.</returns>
        public RequestResult<IEnumerable<Comment>> GetList(string hdId, string parentEntryId);

        /// <summary>
        /// Updates the given Comment in the backend.
        /// Any changes to HDID will be ignored.
        /// </summary>
        /// <param name="comment">The Comment to update.</param>
        /// <returns>The updated Comment.</returns>
        public RequestResult<Comment> Update(Comment comment);

        /// <summary>
        /// Deletes the given note from the backend.
        /// </summary>
        /// <param name="comment">The Comment to be deleted.</param>
        /// <returns>The deleted Comment wrapped in a RequestResult.</returns>
        public RequestResult<Comment> Delete(Comment comment);
    }
}
