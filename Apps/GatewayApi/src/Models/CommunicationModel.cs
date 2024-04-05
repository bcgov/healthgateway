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
namespace HealthGateway.GatewayApi.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;
    using CommunicationStatus = HealthGateway.GatewayApi.Constants.CommunicationStatus;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// Model that provides a user representation of a communication database model.
    /// </summary>
    public class CommunicationModel
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        [JsonPropertyName("CommunicationId")]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Gets the message subject.
        /// </summary>
        public string Subject { get; init; } = string.Empty;

        /// <summary>
        /// Gets the effective datetime.
        /// </summary>
        public DateTime EffectiveDateTime { get; init; }

        /// <summary>
        /// Gets the effective datetime.
        /// </summary>
        public DateTime ExpiryDateTime { get; init; }

        /// <summary>
        /// Gets the scheduled datetime.
        /// </summary>
        public DateTime? ScheduledDateTime { get; init; }

        /// <summary>
        /// Gets the type of the Communication (Banner, Email or In-App).
        /// </summary>
        public CommunicationType CommunicationTypeCode { get; init; } = CommunicationType.Banner;

        /// <summary>
        /// Gets the state of the Communication (Draft, Pending ...).
        /// </summary>
        public CommunicationStatus CommunicationStatusCode { get; init; } = CommunicationStatus.New;

        /// <summary>
        /// Gets the priority of the email communication.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; init; } = EmailPriority.Standard;
    }
}
