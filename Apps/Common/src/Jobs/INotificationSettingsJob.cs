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
namespace HealthGateway.Common.Jobs
{
    /// <summary>
    /// A Job to send/retry pushing Notification Settings to PHSA.
    /// </summary>
    public interface INotificationSettingsJob
    {
        /// <summary>
        /// Sends an email immediately if Priority is standard or higher.
        /// </summary>
        /// <param name="notificationSettingsJSON">The Notification settings serialized to send to PHSA.</param>
        void PushNotificationSettings(string notificationSettingsJSON);
    }
}
