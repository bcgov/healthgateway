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
    using System.Text.Json.Serialization;

    /// <summary>
    /// PHSA Notification Settings request model.
    /// </summary>
    public class NotificationSettingsRequest : NotificationSettingsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsRequest"/> class.
        /// </summary>
        public NotificationSettingsRequest()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsRequest"/> class.
        /// </summary>
        /// <param name="notificationSettings">Initialize values from passed in object.</param>
        public NotificationSettingsRequest(NotificationSettingsBase notificationSettings)
            : base(notificationSettings)
        {
        }

        /// <summary>
        /// Gets or sets the code used to validate the ownership of the number.
        /// </summary>
        [JsonPropertyName("smsVerificationCode")]
        public string? SMSVerificationCode { get; set; }
    }
}
