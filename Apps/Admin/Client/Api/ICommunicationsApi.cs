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
namespace HealthGateway.Admin.Client.Api
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Models;
    using Refit;

    /// <summary>
    /// API for interacting with communications.
    /// </summary>
    public interface ICommunicationsApi
    {
        /// <summary>
        /// Adds a communication.
        /// </summary>
        /// <param name="communication">The model to add.</param>
        /// <returns>The wrapped model.</returns>
        [Post("/")]
        Task<RequestResult<Communication>> AddAsync([Body] Communication communication);

        /// <summary>
        /// Gets all communications.
        /// </summary>
        /// <returns>The wrapped collection of models.</returns>
        [Get("/")]
        Task<RequestResult<IEnumerable<Communication>>> GetAllAsync();

        /// <summary>
        /// Updates a communication.
        /// </summary>
        /// <param name="communication">The model to update.</param>
        /// <returns>The wrapped model.</returns>
        [Put("/")]
        Task<RequestResult<Communication>> UpdateAsync([Body] Communication communication);

        /// <summary>
        /// Deletes a communication.
        /// </summary>
        /// <param name="communication">The model to delete.</param>
        /// <returns>The wrapped model.</returns>
        [Delete("/")]
        Task<RequestResult<Communication>> DeleteAsync([Body] Communication communication);
    }
}
