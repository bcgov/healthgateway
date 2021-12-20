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
namespace HealthGateway.Common.Delegates.PHSA
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Interface that defines a delegate to get and set Notification Settings at PHSA.
    /// </summary>
    public interface INotificationSettingsDelegate
    {
        /// <summary>
        /// Gets the Notification Settings for authenticated user.
        /// </summary>
        /// <param name="bearerToken">The access token of the authenticated user.</param>
        /// <returns>The Notification Settings wrapped in a RequestResult.</returns>
        Task<RequestResult<NotificationSettingsResponse>> GetNotificationSettings(string bearerToken);

        /// <summary>
        /// Creates or Updates the Notification Settings at PHSA.
        /// </summary>
        /// <param name="notificationSettings">The notification settings request to send to PHSA.</param>
        /// <param name="bearerToken">The access token of the authenticated user.</param>
        /// <returns>The notification settings response returned from PHSA wrapped in a RequestResult.</returns>
        Task<RequestResult<NotificationSettingsResponse>> SetNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken);
    }
}
