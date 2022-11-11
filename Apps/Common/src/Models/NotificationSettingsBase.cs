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
        protected NotificationSettingsBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsBase"/> class.
        /// </summary>
        /// <param name="notificationSettings">Initialize values from passed in object.</param>
        protected NotificationSettingsBase(NotificationSettingsBase notificationSettings)
        {
            this.SubjectHdid = notificationSettings.SubjectHdid;
            this.SmsEnabled = notificationSettings.SmsEnabled;
            this.SmsNumber = notificationSettings.SmsNumber;
            this.SmsVerified = notificationSettings.SmsVerified;
            this.SmsScope = notificationSettings.SmsScope.ToList();
            this.EmailAddress = notificationSettings.EmailAddress;
            this.EmailEnabled = notificationSettings.EmailEnabled;
            this.EmailScope = notificationSettings.EmailScope.ToList();
        }

        /// <summary>
        /// Gets or sets a value indicating the subject HDID. This is an identifier person this notification is sent to.
        /// </summary>
        [JsonPropertyName("subjectHdid")]
        public string? SubjectHdid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS notifications are enabled.
        /// </summary>
        [JsonPropertyName("smsEnabled")]
        public bool SmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number to be used for SMS notifications.
        /// </summary>
        [JsonPropertyName("smsCellNumber")]
        public string? SmsNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS number has been validated.
        /// </summary>
        [JsonPropertyName("smsVerified")]
        public bool SmsVerified { get; set; }

        /// <summary>
        /// Gets or sets the SMS scope.
        /// </summary>
        [JsonPropertyName("smsScope")]
        public IEnumerable<NotificationTarget> SmsScope { get; set; } = new List<NotificationTarget> { NotificationTarget.Covid19 };

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
