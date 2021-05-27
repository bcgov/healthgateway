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
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Represents a Wallet Connection.
    /// </summary>
    public class WalletConnection : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the Wallet connection unique id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("WalletConnectionId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Users HdId.
        /// </summary>
        [Required]
        [MaxLength(54)]
        public string UserProfileId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the UserProfile associated to this Wallet Connection.
        /// </summary>
        public virtual UserProfile? UserProfile { get; set; }

        /// <summary>
        /// Gets or sets the status of the connection.
        /// </summary>
        [Required]
        [MaxLength(15)]
        public WalletConnectionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Agent Connection Id.
        /// </summary>
        public string? AgentId { get; set; }

        /// <summary>
        /// Gets or sets the invitation url from the issuer.
        /// </summary>
        public string? InvitationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the datetime the connection was established.
        /// </summary>
        public DateTime? ConnectedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the datetime the connection was removed.
        /// </summary>
        public DateTime? DisconnectedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the collection of credentials for this connection.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team Decision")]
        public ICollection<WalletCredential> Credentials { get; set; } = null!;
    }
}
