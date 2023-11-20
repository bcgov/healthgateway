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
namespace HealthGateway.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// The DelegateInvitation model.
    /// </summary>
    public class DelegateInvitation : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Key]
        [Column("DelegateInvitationId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Nickname { get; set; } = null!;

        /// <summary>
        /// Gets or sets the status for the delegate invitation.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public DelegateInvitationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the sharing code hash function.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public HashFunction SharingCodeHashFunction { get; set; }

        /// <summary>
        /// Gets or sets the sharing code iterations.
        /// </summary>
        [Required]
        public int SharingCodeIterations { get; set; }

        /// <summary>
        /// Gets or sets the sharing code salt.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string SharingCodeSalt { get; set; } = null!;

        /// <summary>
        /// Gets or sets the sharing code hash.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string SharingCodeHash { get; set; } = null!;

        /// <summary>
        /// Gets or sets the failed attempts.
        /// </summary>
        [Required]
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        [Required]
        public DateOnly ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the access for the data sets.
        /// </summary>
        [Column(TypeName = "jsonb")]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public HashSet<DataSource>? DataSources { get; set; }

        /// <summary>
        /// Gets or sets the resource owner's HDID.
        /// </summary>
        [Required]
        [MaxLength(52)]
        public string ResourceOwnerHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the delegate's HDID.
        /// </summary>
        [MaxLength(52)]
        public string? ProfileHdid { get; set; }

        /// <summary>
        /// Gets or sets the reason code for the delegation.
        /// </summary>
        [MaxLength(10)]
        public ResourceDelegateReason? ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets the resource owner identifier.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ResourceOwnerIdentifier { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the delegation invitation was removed by the owner.
        /// </summary>
        [Required]
        public bool RemovedByOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the delegation invitation was removed by the delegate.
        /// </summary>
        [Required]
        public bool RemovedByDelegate { get; set; }

        /// <summary>
        /// Gets or sets the resource delegate.
        /// </summary>
        public virtual ResourceDelegate ResourceDelegate { get; set; } = null!;
    }
}
