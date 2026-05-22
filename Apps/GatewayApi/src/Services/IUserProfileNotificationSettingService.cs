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
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Provides business logic operations for user profile notification settings.
    /// </summary>
    public interface IUserProfileNotificationSettingService
    {
        /// <summary>
        /// Retrieves all notification settings models for the specified user.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The collection of notification settings.</returns>
        Task<IList<UserProfileNotificationSettingModel>> GetAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Updates the notification setting for a specific notification type.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="model">
        /// The notification setting model containing the notification type and optional delivery channel values.
        /// Only provided channel values are updated and included in the emitted notification preference event.
        /// </param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(
            string hdid,
            UserProfileNotificationSettingModel model,
            CancellationToken ct = default);

        /// <summary>
        /// Updates notification settings for one or more notification types.
        /// </summary>
        /// <param name="hdid">The user HDID.</param>
        /// <param name="models">
        /// A collection of notification setting models containing the notification types and optional delivery channel values.
        /// Provided channel values are updated. When a channel value is not provided, an existing stored value may still be
        /// included in the emitted notification preference event.
        /// </param>
        /// <param name="commit">Whether to commit changes immediately or defer to the caller's transaction.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(
            string hdid,
            IReadOnlyCollection<UserProfileNotificationSettingModel> models,
            bool commit = true,
            CancellationToken ct = default);
    }
}
