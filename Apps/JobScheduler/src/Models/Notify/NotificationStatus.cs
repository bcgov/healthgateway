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
namespace HealthGateway.JobScheduler.Models.Notify
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the status of a notification (Email or SMS).
    /// </summary>
    public class NotificationStatus
    {
        /// <summary>
        /// Gets or sets the notification id.
        /// </summary>
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the reference id.
        /// </summary>
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        /// <summary>
        /// Gets or sets the email address the notification was sent to.
        /// </summary>
        [JsonPropertyName("email_address")]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the phone number the notification was sent to.
        /// </summary>
        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of notification.
        /// valid values are: email, sms.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the status of the notification.
        /// valid values are: created, sending, delivered, permanent-failure, temporary-failure, technical-failure.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the notification provider response.
        /// Will be set only if the the status is is technical-failure.
        /// </summary>
        [JsonPropertyName("provider_response")]
        public string? ProviderResponse { get; set; }

        /// <summary>
        /// Gets or sets the template used for the notification.
        /// </summary>
        [JsonPropertyName("template")]
        public GcTemplate? Template { get; set; }

        /// <summary>
        /// Gets or sets the body of the notification.
        /// </summary>
        [JsonPropertyName("body")]
        public string? Body { get; set; }

        /// <summary>
        /// Gets or sets the subject of the notification.
        /// </summary>
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the created date of the notification.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the Sent date of the notification.
        /// </summary>
        [JsonPropertyName("sent_at")]
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Gets or sets the completed date of the notification.
        /// </summary>
        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }
}
