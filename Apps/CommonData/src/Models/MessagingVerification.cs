//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.Constants;

#pragma warning disable CS1591 // self explanatory simple model
#pragma warning disable SA1600 // self explanatory simple model
    [ExcludeFromCodeCoverage]
    public class MessagingVerification : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the messaging verification id.
        /// </summary>
        [Key]
        [Column("MessagingVerificationId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the users directed identifier.
        /// </summary>
        [Required]
        [MaxLength(52)]
        [Column("HdId")]
        public string? UserProfileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the messaging verification was validated.
        /// </summary>
        [Required]
        public bool Validated { get; set; }

        /// <summary>
        /// Gets or sets the associated email that was sent for this verification.
        /// Required if the VerificationType = MessagingVerificationType.Email.
        /// </summary>
        public Guid? EmailId { get; set; }

        /// <summary>
        /// Gets or sets the associated email for this verification.
        /// Required if the VerificationType = MessagingVerificationType.Email.
        /// </summary>
        public virtual Email? Email { get; set; }

        /// <summary>
        /// Gets or sets the email invite key.
        /// </summary>
        [Required]
        public Guid InviteKey { get; set; }

        /// <summary>
        /// Gets or sets the Verification type as defined by MessagingVerificationTypeCode.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string VerificationType { get; set; } = MessagingVerificationType.Email;

        /// <summary>
        /// Gets or sets the SMS number for this verification.
        /// Required if the VerificationType = MessagingVerificationType.SMS.
        /// </summary>
        [MaxLength(10)]
        [Column("SMSNumber")]
        public string? SmsNumber { get; set; }

        /// <summary>
        /// Gets or sets the SMS validation code for this verification.
        /// </summary>
        [MaxLength(6)]
        [Column("SMSValidationCode")]
        public string? SmsValidationCode { get; set; }

        /// <summary>
        /// Gets or sets the expire date for the messaging verification.
        /// </summary>
        [Required]
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the attempted verification count.
        /// </summary>
        [Required]
        public int VerificationAttempts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the messaging verification was deleted.
        /// </summary>
        [Required]
        public bool Deleted { get; set; }
    }
}
