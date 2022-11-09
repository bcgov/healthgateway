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
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Represents a historical view of the <see cref="UserProfile"/> class.
    /// This object needs to be in-sync with the UserProfile object.
    /// This object only exists for code first database generation.
    /// </summary>
    public class UserProfileHistory : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        [Column("UserProfileId")]
        [MaxLength(52)]
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the user accepted the terms of service.
        /// </summary>
        public bool? AcceptedTermsOfService { get; set; }

        /// <summary>
        /// Gets or sets the LegalAgreement ToS Id.
        /// This is not maintained as a foreign key unlike UserProfile.
        /// </summary>
        public Guid? TermsOfServiceId { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        [MaxLength(254)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the user SMS number.
        /// </summary>
        [MaxLength(10)]
        [Column("SMSNumber")]
        public string? SmsNumber { get; set; }

        /// <summary>
        /// Gets or sets the Closed datetime of the account.
        /// After an account has been closed for n amount of days the row is physically deleted.
        /// </summary>
        public DateTime? ClosedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the External Identity Management identifier for the user.
        /// </summary>
        public Guid? IdentityManagementId { get; set; }

        /// <summary>
        /// Gets or sets the users last login datetime.
        /// </summary>
        public DateTime LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the users encryption key.
        /// Key is 16 byte string and is encoded to Base64.
        /// </summary>
        [MaxLength(44)]
        public string? EncryptionKey { get; set; }

        /// <summary>
        /// Gets or sets the user's year of birth.
        /// </summary>
        [MaxLength(4)]
        public string? YearOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Column("UserProfileHistoryId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the operation that created this history row.
        /// </summary>
        [Required]
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the datetime the operation was performed.
        /// </summary>
        [Required]
        public DateTime OperationDateTime { get; set; } = DateTime.MaxValue;
    }
}
