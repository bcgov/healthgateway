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
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.WebClient.Models;

    /// <summary>
    /// Service to interact with the Comment Delegate.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Adds a UserComment in the backend.
        /// </summary>
        /// <param name="userComment">The UserComment to be created.</param>
        /// <returns>A UserComment wrapped in a RequestResult.</returns>
        RequestResult<UserComment> Add(UserComment userComment);

        /// <summary>
        /// Gets a list of UserComment for the given hdId and event id.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <returns>A List of UserComment wrapped in a RequestResult.</returns>
        RequestResult<IEnumerable<UserComment>> GetEntryComments(string hdId, string parentEntryId);

        /// <summary>
        /// Gets a list of UserComment for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <returns>A List of UserComment wrapped in a RequestResult.</returns>
        RequestResult<IDictionary<string, IEnumerable<UserComment>>> GetProfileComments(string hdId);

        /// <summary>
        /// Updates the given UserComment in the backend.
        /// Any changes to HDID will be ignored.
        /// </summary>
        /// <param name="userComment">The UserComment to update.</param>
        /// <returns>The updated UserComment.</returns>
        RequestResult<UserComment> Update(UserComment userComment);

        /// <summary>
        /// Deletes the given UserComment from the backend.
        /// </summary>
        /// <param name="userComment">The Comment to be deleted.</param>
        /// <returns>The deleted Comment wrapped in a RequestResult.</returns>
        RequestResult<UserComment> Delete(UserComment userComment);
    }
}
