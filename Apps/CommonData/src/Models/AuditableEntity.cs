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
    using System.Runtime.Serialization;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Base class for all DB entities to ensure audit data is saved.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class AuditableEntity : IAuditable, IConcurrencyGuard
    {
        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        [Required]
        [MaxLength(60)]
        [IgnoreDataMember]
        public string CreatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was created.
        /// This is generally set by the baseDbContext.
        /// </summary>
        [Required]
        [IgnoreDataMember]
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        [Required]
        [MaxLength(60)]
        [IgnoreDataMember]
        public string UpdatedBy { get; set; } = UserId.DefaultUser;

        /// <summary>
        /// Gets or sets the datetime the entity was updated.
        /// This is generally set by the baseDbContext.
        /// </summary>
        [Required]
        [IgnoreDataMember]
        public DateTime UpdatedDateTime { get; set; }

        /// <inheritdoc/>
        [ConcurrencyCheck]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("xmin", TypeName = "xid")]
        public uint Version { get; set; }
    }
}
