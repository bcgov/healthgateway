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
namespace HealthGateway.Common.Models
{
    /// <summary>
    /// Provides configuration data for the PHSA notifications settings API.
    /// </summary>
    public class NotificationSettingsConfig
    {
        /// <summary>
        /// Configuration section key for the PHSA notification settings API.
        /// </summary>
        public const string NotificationSettingsConfigSectionKey = "NotificationSettings";

        /// <summary>
        /// Gets or sets the external endpoint for the PHSA notification settings API.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;
    }
}
