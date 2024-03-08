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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Service to interact with the Comment Delegate.
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Adds a UserComment in the backend.
        /// </summary>
        /// <param name="userComment">The UserComment to be created.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A UserComment wrapped in a RequestResult.</returns>
        Task<RequestResult<UserComment>> AddAsync(UserComment userComment, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of UserComment for the given hdId and event id.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A List of UserComment wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<UserComment>>> GetEntryCommentsAsync(string hdId, string parentEntryId, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of UserComment for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A List of UserComment wrapped in a RequestResult.</returns>
        Task<RequestResult<IDictionary<string, IEnumerable<UserComment>>>> GetProfileCommentsAsync(string hdId, CancellationToken ct = default);

        /// <summary>
        /// Updates the given UserComment in the backend.
        /// Any changes to HDID will be ignored.
        /// </summary>
        /// <param name="userComment">The UserComment to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated UserComment.</returns>
        Task<RequestResult<UserComment>> UpdateAsync(UserComment userComment, CancellationToken ct = default);

        /// <summary>
        /// Deletes the given UserComment from the backend.
        /// </summary>
        /// <param name="userComment">The Comment to be deleted.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The deleted Comment wrapped in a RequestResult.</returns>
        Task<RequestResult<UserComment>> DeleteAsync(UserComment userComment, CancellationToken ct = default);
    }
}
