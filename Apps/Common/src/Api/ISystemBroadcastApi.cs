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
namespace HealthGateway.Common.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA System Broadcasts API.
    /// </summary>
    public interface ISystemBroadcastApi
    {
        /// <summary>
        /// Retrieves broadcasts.
        /// </summary>
        /// <returns>A response containing the collection of broadcasts.</returns>
        [Get("/api/system-broadcasts")]
        Task<IApiResponse<IEnumerable<BroadcastResponse>>> GetBroadcasts();

        /// <summary>
        /// Creates a broadcast.
        /// </summary>
        /// <param name="request">The broadcast to create.</param>
        /// <returns>A response containing the broadcast that was created.</returns>
        [Post("/api/system-broadcasts")]
        Task<IApiResponse<BroadcastResponse>> CreateBroadcast([Body] BroadcastRequest request);

        /// <summary>
        /// Updates a broadcast.
        /// </summary>
        /// <param name="id">The id of the broadcast that is being updated.</param>
        /// <param name="request">The broadcast values to update.</param>
        /// <returns>A response containing the broadcast that was updated.</returns>
        [Put("/api/system-broadcasts/{id}")]
        Task<IApiResponse<BroadcastResponse>> UpdateBroadcast(string id, [Body] BroadcastRequest request);

        /// <summary>
        /// Deletes a broadcast.
        /// </summary>
        /// <param name="id">The id of the broadcast that is being deleted.</param>
        /// <returns>A response indicating success or failure.</returns>
        [Delete("/api/system-broadcasts/{id}")]
        Task<IApiResponse> DeleteBroadcast(string id);
    }
}
