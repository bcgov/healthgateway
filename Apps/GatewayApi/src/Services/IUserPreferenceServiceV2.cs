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
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The user preference service.
    /// </summary>
    public interface IUserPreferenceServiceV2
    {
        /// <summary>
        /// Updates a user preference.
        /// </summary>
        /// <param name="hdid">The hdid associated with the user.</param>
        /// <param name="userPreferenceModel">The user preference to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The updated user preference model.</returns>
        Task<UserPreferenceModel> UpdateUserPreferenceAsync(string hdid, UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Creates a user preference.
        /// </summary>
        /// <param name="hdid">The hdid associated with the user.</param>
        /// <param name="userPreferenceModel">The user preference to create.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The created user preference model.</returns>
        Task<UserPreferenceModel> CreateUserPreferenceAsync(string hdid, UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Gets preferences associated with a user.
        /// </summary>
        /// <param name="hdid">The hdid associated with the user.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request</param>
        /// <returns>A dictionary mapping preference names with their associated user preference models. </returns>
        Task<Dictionary<string, UserPreferenceModel>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default);
    }
}
