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
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Represents a text message template.
    /// </summary>
    public class EmailTemplate : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the email template unique id.
        /// </summary>
        [Key]
        [Column("EmailTemplateId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the template.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the From address for sending the email.
        /// </summary>
        [Required]
        [MaxLength(254)]
        public string? From { get; set; }

        /// <summary>
        /// Gets or sets the subject line.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the Body.
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string? Body { get; set; }

        /// <summary>
        /// Gets or sets the priority of the email.
        /// The lower the value the lower the priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the template effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the template expiry date.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the email format.
        /// </summary>
        [Required]
        [MaxLength(4)]
        public EmailFormat FormatCode { get; set; } = EmailFormat.Text;
    }
}
