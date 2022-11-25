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
namespace HealthGateway.GatewayApi.Models.Phsa
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Model representing a PHSA web alert.
    /// </summary>
    public class PhsaWebAlert
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [JsonPropertyName("type")]
        public PhsaWebAlertType Type { get; set; }

        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        [JsonPropertyName("displayText")]
        public string? DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the action URL.
        /// </summary>
        [JsonPropertyName("actionUrl")]
        public Uri? ActionUrl { get; set; }

        /// <summary>
        /// Gets or sets the action type.
        /// </summary>
        [JsonPropertyName("actionType")]
        public BroadcastActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        [JsonPropertyName("creator")]
        public string? Creator { get; set; }

        /// <summary>
        /// Gets or sets the target ID.
        /// </summary>
        [JsonPropertyName("targetId")]
        public string? TargetId { get; set; }

        /// <summary>
        /// Gets or sets the scheduled datetime in UTC.
        /// </summary>
        [JsonPropertyName("scheduledDateUtc")]
        public DateTime ScheduledDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the expiration datetime in UTC.
        /// </summary>
        [JsonPropertyName("expirationDateUtc")]
        public DateTime ExpirationDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the created datetime in UTC.
        /// </summary>
        [JsonPropertyName("creationDateUtc")]
        public DateTime CreationDateTimeUtc { get; set; }
    }
}
