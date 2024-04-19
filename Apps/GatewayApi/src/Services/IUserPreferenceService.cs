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
    /// The User Preference service.
    /// </summary>
    public interface IUserPreferenceService
    {
        /// <summary>
        /// Updates a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to update.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        Task<RequestResult<UserPreferenceModel>> UpdateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Create a User Preference in the backend.
        /// </summary>
        /// <param name="userPreferenceModel">The user preference to create.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A userPreference wrapped in a RequestResult.</returns>
        Task<RequestResult<UserPreferenceModel>> CreateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default);

        /// <summary>
        /// Gets user preferences associated with the hdid.
        /// </summary>
        /// <param name="hdid">The hdid associated with the user preferences.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request</param>
        /// <returns>A dictionary mapping preference with it's associated user preference model. </returns>
        Task<Dictionary<string, UserPreferenceModel>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default);
    }
}
