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

    /// <summary>
    /// Represents an Email to send from the system.
    /// </summary>
    public class Email : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the primary key of this Email entity.
        /// </summary>
        [Key]
        [Column("EmailId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the From address for sending the email.
        /// </summary>
        [Required]
        [MaxLength(254)]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the To address for sending the email.
        /// </summary>
        [Required]
        [MaxLength(254)]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the Subject line of the email.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Body of the email.
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the priority of the email.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time the email was sent.
        /// </summary>
        public DateTime? SentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time we last tried to send the email.
        /// </summary>
        public DateTime? LastRetryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of times we have tried sending this email.
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the Email Format (HTML/Text).
        /// </summary>
        [Required]
        [MaxLength(4)]
        public string FormatCode { get; set; }

        /// <summary>
        /// Gets or sets the EmailFormatCode object.
        /// </summary>
        public virtual EmailFormatCode Format { get; set; }
    }
}