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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Constants;

    /// <summary>
    /// PHSA Notification Settings base request/response model.
    /// </summary>
    public abstract class NotificationSettingsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsBase"/> class.
        /// </summary>
        public NotificationSettingsBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsBase"/> class.
        /// </summary>
        /// <param name="notificationSettings">Initialize values from passed in object.</param>
        public NotificationSettingsBase(NotificationSettingsBase notificationSettings)
        {
            this.SMSEnabled = notificationSettings.SMSEnabled;
            this.SMSNumber = notificationSettings.SMSNumber;
            this.SMSVerified = notificationSettings.SMSVerified;
            this.SMSScope = notificationSettings.SMSScope.ToList();
            this.EmailAddress = notificationSettings.EmailAddress;
            this.EmailEnabled = notificationSettings.EmailEnabled;
            this.EmailScope = notificationSettings.EmailScope.ToList();
        }

        /// <summary>
        /// Gets or sets a value indicating whether SMS notifications are enabled.
        /// </summary>
        [JsonPropertyName("smsEnabled")]
        public bool SMSEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number to be used for SMS notifications.
        /// </summary>
        [JsonPropertyName("smsCellNumber")]
        public string? SMSNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS number has been validated.
        /// </summary>
        [JsonPropertyName("smsVerified")]
        public bool SMSVerified { get; set; }

        /// <summary>
        /// Gets or sets the SMS scope.
        /// </summary>
        [JsonPropertyName("smsScope")]
        public IEnumerable<NotificationTarget> SMSScope { get; set; } = new List<NotificationTarget> { NotificationTarget.Covid19 };

        /// <summary>
        /// Gets or sets a value indicating whether Email notifications are enabled.
        /// </summary>
        [JsonPropertyName("emailEnabled")]
        public bool EmailEnabled { get; set; }

        /// <summary>
        /// Gets or sets the email to be used for notifications.
        /// </summary>
        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the Email scope.
        /// </summary>
        [JsonPropertyName("emailScope")]
        public IEnumerable<NotificationTarget> EmailScope { get; set; } = new List<NotificationTarget> { NotificationTarget.Covid19 };
    }
}
