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
namespace HealthGateway.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// A system Communication.
    /// </summary>
    public class Communication : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CommunicationId")]
        [JsonPropertyName("CommunicationId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message subject.
        /// </summary>
        [MaxLength(1000)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        [Required]
        public DateTime EffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the effective datetime.
        /// </summary>
        [Required]
        public DateTime ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the scheduled datetime.
        /// </summary>
        public DateTime? ScheduledDateTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the Communication (Banner, Email or In-App).
        /// </summary>
        [MaxLength(10)]
        public CommunicationType CommunicationTypeCode { get; set; } = CommunicationType.Banner;

        /// <summary>
        /// Gets or sets the state of the Communication (Draft, Pending ...).
        /// </summary>
        [MaxLength(10)]
        public CommunicationStatus CommunicationStatusCode { get; set; } = CommunicationStatus.New;

        /// <summary>
        /// Gets or sets the priority of the email communication.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; } = EmailPriority.Standard;
    }
}
