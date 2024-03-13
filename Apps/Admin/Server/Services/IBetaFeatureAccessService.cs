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
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Service to manage beta feature access.
    /// </summary>
    public interface IBetaFeatureAccessService
    {
        /// <summary>
        /// Adds or deletes beta features associated with the supplied email to the database.
        /// </summary>
        /// <param name="email">The email associated with the user profile.</param>
        /// <param name="betaFeatures">The list of beta features associated with the supplied email.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetUserAccessAsync(string email, IList<BetaFeature> betaFeatures, CancellationToken ct = default);

        /// <summary>
        /// Returns a list of available beta features associated with the supplied email.
        /// </summary>
        /// <param name="email">The email associated with the user profile to search on.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The list of available beta features.</returns>
        Task<IEnumerable<BetaFeature>> GetUserAccessAsync(string email, CancellationToken ct = default);
    }
}