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
namespace HealthGateway.Common.Models.Events
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Messaging;

    /// <summary>
    /// Represents a notification data source.
    /// An empty collection indicates no targets.
    /// </summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum NotificationTargets
    {
        /// <summary>
        /// Unknown notification target.
        /// This value exists as the default enum value and as a fallback
        /// during deserialization when an unrecognized value is received.
        /// It must not be intentionally emitted in outgoing messages,
        /// as receiving systems do not recognize it as a valid target.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Laboratory results notifications.
        /// </summary>
        Laboratory = 1,

        /// <summary>
        /// Immunization record notifications.
        /// </summary>
        Immunization = 2,

        /// <summary>
        /// COVID-19 laboratory result notifications.
        /// </summary>
        Covid19Laboratory = 3,

        /// <summary>
        /// COVID-19 immunization record notifications.
        /// </summary>
        Covid19Immunization = 4,

        /// <summary>
        /// Medication record notifications.
        /// </summary>
        Medications = 5,

        /// <summary>
        /// Health visit notifications.
        /// </summary>
        HealthVisits = 6,

        /// <summary>
        /// Special Authority notifications.
        /// </summary>
        SpecialAuthority = 7,

        /// <summary>
        /// Broadcast message notifications.
        /// </summary>
        Broadcasts = 8,

        /// <summary>
        /// BC Cancer screening notifications.
        /// </summary>
        BcCancer = 9,
    }

    /// <summary>
    /// Raised when a user's notification channel preferences are changed.
    /// </summary>
    /// <param name="Hdid">
    /// The user's HDID (Health Data ID).
    /// </param>
    /// <param name="SmsNumber">
    /// The SMS phone number associated with the user, if available.
    /// </param>
    /// <param name="SmsNotificationTargets">
    /// The notification targets enabled for SMS delivery.
    /// </param>
    /// <param name="Email">
    /// The email address associated with the user, if available.
    /// </param>
    /// <param name="EmailNotificationTargets">
    /// The notification targets enabled for email delivery.
    /// </param>
    public record NotificationChannelPreferencesChangedEvent(
        string Hdid,
        string? SmsNumber,
        IReadOnlyCollection<NotificationTargets> SmsNotificationTargets,
        string? Email,
        IReadOnlyCollection<NotificationTargets> EmailNotificationTargets)
        : MessageBase;
}
