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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// The resource delegate model.
    /// </summary>
    public class ResourceDelegate : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the HDID corresponding to the resource owner (the dependent).
        /// </summary>
        [MaxLength(52)]
        public string ResourceOwnerHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the HDID corresponding to the user with delegated access to the resources (the delegate).
        /// </summary>
        [MaxLength(52)]
        public string ProfileHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the reason code for the delegation.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public ResourceDelegateReason ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets the resource delegate's expiry date.
        /// </summary>
        [Required]
        public DateOnly ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the ReasonObject type.
        /// This value is used by code to reconstruct the JSON POCO.
        /// </summary>
        public string? ReasonObjectType { get; set; }

        /// <summary>
        /// Gets or sets the resource delegation Reason object.
        /// </summary>
        public JsonDocument? ReasonObject { get; set; }

        /// <summary>
        /// Gets or sets the UserProfile associated with <see cref="ProfileHdid"/>.
        /// </summary>
        public virtual UserProfile UserProfile { get; set; } = null!;

        /// <summary>
        /// Gets or sets the access for the data sets.
        /// </summary>
        [Column(TypeName = "jsonb")]
        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Team decision")]
        public HashSet<DataSource>? DataSources { get; set; }

        /// <summary>
        /// Gets the delegate invitations.
        /// </summary>
        public virtual ICollection<DelegateInvitation>? DelegateInvitations { get; } = new List<DelegateInvitation>();
    }
}
