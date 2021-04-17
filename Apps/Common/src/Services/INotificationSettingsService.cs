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
namespace HealthGateway.Common.Services
{
    using HealthGateway.Common.Models;

    /// <summary>
    /// Provides a mechanism to push notification settings to PHSA.
    /// </summary>
    public interface INotificationSettingsService
    {
        /// <summary>
        /// Queues pushing the Notification Settings to PHSA using our batch system.
        /// Will use access_token acquired from system account authenication.
        /// </summary>
        /// <param name="notificationSettings">The Notification Settings Request object.</param>
        void QueueNotificationSettings(NotificationSettingsRequest notificationSettings);
    }
}
