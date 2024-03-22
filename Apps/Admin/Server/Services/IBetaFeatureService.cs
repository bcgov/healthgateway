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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Service to manage beta feature access.
    /// </summary>
    public interface IBetaFeatureService
    {
        /// <summary>
        /// Sets access to beta features for users with the provided email address.
        /// </summary>
        /// <param name="access">Model consisting of an email address and collection of available beta features.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetUserAccessAsync(UserBetaAccess access, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the beta features available for users with the provided email address.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A model containing the beta features available for users with the provided email address.</returns>
        Task<UserBetaAccess> GetUserAccessAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a collection containing the emails of all users with beta access and the beta features associated with them.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A collection containing the emails of all users with beta access and the beta features associated with them.</returns>
        Task<IEnumerable<UserBetaAccess>> GetAllUserAccessAsync(CancellationToken ct = default);
    }
}
