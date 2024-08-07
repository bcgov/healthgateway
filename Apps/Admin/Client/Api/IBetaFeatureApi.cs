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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Models;
    using Refit;

    /// <summary>
    /// API for managing beta features available to users.
    /// </summary>
    public interface IBetaFeatureApi
    {
        /// <summary>
        /// Sets access to beta features for users with the provided email address.
        /// </summary>
        /// <param name="request">Request model consisting of an email address and collection of available beta features.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Put("/UserAccess")]
        Task SetUserAccessAsync([Body] UserBetaAccess request);

        /// <summary>
        /// Retrieves the beta features available for users with the provided email address.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>A model containing the beta features available for users with the provided email address.</returns>
        [Get("/UserAccess")]
        Task<UserBetaAccess> GetUserAccessAsync(string email);

        /// <summary>
        /// Retrieves the emails of all users with beta access and the beta features associated with them, using pagination.
        /// </summary>
        /// <param name="pageIndex">Current page index, starting from 0.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection containing the emails of all users with beta access and the beta features associated with them.</returns>
        [Get("/AllUserAccess")]
        Task<PaginatedResult<UserBetaAccess>> GetAllUserAccessAsync(int pageIndex, int pageSize, CancellationToken ct = default);
    }
}
