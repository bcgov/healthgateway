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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Represents a Wallet Credential.
    /// </summary>
    public class WalletCredential : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the Wallet credential unique id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("WalletCredentialId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Wallet Connection Id (foreign key).
        /// </summary>
        [ForeignKey("WalletConnection")]
        [Required]
        public Guid WalletConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the Wallet Connection object associated to the credential.
        /// </summary>
        public virtual WalletConnection WalletConnection { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the resource that this credential is attached to.
        /// </summary>
        public string ResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type the resource that this credential is attached to.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the credential.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public WalletCredentialStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the datetime the credential was established.
        /// </summary>
        public DateTime? AddedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the datetime the credential was removed.
        /// </summary>
        public DateTime? RevokedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the exchange id for this credential.
        /// </summary>
        public Guid ExchangeId { get; set; }

        /// <summary>
        /// Gets or sets the revocation id from the agent.
        /// </summary>
        public string? RevocationId { get; set; }

        /// <summary>
        /// Gets or sets the revocation registry id from the agent.
        /// </summary>
        public string? RevocationRegistryId { get; set; }
    }
}
