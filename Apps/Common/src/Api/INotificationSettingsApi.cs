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
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using Refit;

    /// <summary>
    /// Refit interface to interact with the notification settings API at PHSA.
    /// </summary>
    public interface INotificationSettingsApi
    {
        /// <summary>
        /// Creates or updates notification settings.
        /// </summary>
        /// <param name="request">The notification settings request to be sent.</param>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="token">The bearer token to authorize the request.</param>
        /// <returns>The notification settings response received.</returns>
        [Put("/")]
        Task<NotificationSettingsResponse> SetNotificationSettingsAsync([Body] NotificationSettingsRequest request, [Header("patient")] string hdid, [Authorize] string token);
    }
}
