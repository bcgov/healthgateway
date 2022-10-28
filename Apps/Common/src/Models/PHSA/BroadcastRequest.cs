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
namespace HealthGateway.Common.Models.PHSA
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Model representing a PHSA Broadcast Request.
    /// </summary>
    public class BroadcastRequest
    {
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the display text.
        /// </summary>
        [JsonPropertyName("displayText")]
        public string? DisplayText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this broadcast system is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the action type.
        /// </summary>
        [JsonPropertyName("actionType")]
        public BroadcastActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the action url.
        /// </summary>
        [JsonPropertyName("actionUrl")]
        public Uri? ActionUrl { get; set; }

        /// <summary>
        /// Gets or sets the scheduled datetime in UTC.
        /// </summary>
        [JsonPropertyName("scheduledDateUtc")]
        public DateTime ScheduledDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the expiration datetime in UTC.
        /// </summary>
        [JsonPropertyName("expirationDateUtc")]
        public DateTime? ExpirationDateUtc { get; set; }
    }
}
