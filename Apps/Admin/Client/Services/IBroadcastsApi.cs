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
namespace HealthGateway.Admin.Client.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using Refit;

    /// <summary>
    /// API for interacting with broadcasts.
    /// </summary>
    public interface IBroadcastsApi
    {
        /// <summary>
        /// Adds a broadcast.
        /// </summary>
        /// <param name="broadcast">The model to add.</param>
        /// <returns>The Broadcast model.</returns>
        [Post("/")]
        Task<RequestResult<Broadcast>> Add([Body] Broadcast broadcast);

        /// <summary>
        /// Gets all broadcasts.
        /// </summary>
        /// <returns>The collection of models.</returns>
        [Get("/")]
        Task<RequestResult<IEnumerable<Broadcast>>> GetAll();

        /// <summary>
        /// Updates a broadcast.
        /// </summary>
        /// <param name="broadcast">The model to update.</param>
        /// <returns>The wrapped model.</returns>
        [Put("/")]
        Task<RequestResult<Broadcast>> Update([Body] Broadcast broadcast);

        /// <summary>
        /// Deletes a broadcast.
        /// </summary>
        /// <param name="broadcast">The model to delete.</param>
        /// <returns>The wrapped model.</returns>
        [Delete("/")]
        Task<RequestResult<Broadcast>> Delete([Body] Broadcast broadcast);
    }
}
