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
namespace HealthGateway.Common.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Represents an Email to send from the system.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Gets or sets the primary key of this Email entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the From address for sending the email.
        /// </summary>
        [Required]
        public string? From { get; set; }

        /// <summary>
        /// Gets or sets the To address for sending the email.
        /// </summary>
        [Required]
        [MaxLength(254)]
        public string? To { get; set; }

        /// <summary>
        /// Gets or sets the Subject line of the email.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the Body of the email.
        /// </summary>
        [Required]
        public string? Body { get; set; }

        /// <summary>
        /// Gets or sets the Email Format (HTML/Text).
        /// </summary>
        [Required]
        [MaxLength(4)]
        public EmailFormat FormatCode { get; set; } = EmailFormat.Text;

        /// <summary>
        /// Gets or sets the priority of the email.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; } = EmailPriority.Standard;

        /// <summary>
        /// Gets or sets the Date/Time the email was sent.
        /// </summary>
        public DateTime? SentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Date/Time we last tried to send the email.
        /// </summary>
        public DateTime? LastRetryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of attempts in sending this email.
        /// </summary>
        public int Attempts { get; set; }

        /// <summary>
        /// Gets or sets the SMTP Status code from the last send attempt.
        /// </summary>
        public int SmtpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the state of the Email (New, Pending ...).
        /// </summary>
        [Required]
        [MaxLength(10)]
        public EmailStatus EmailStatusCode { get; set; } = EmailStatus.New;
    }
}
