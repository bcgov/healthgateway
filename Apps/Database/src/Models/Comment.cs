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
    /// A user entered Comment.
    /// </summary>
    public class Comment : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CommentId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        [MaxLength(52)]
        [Required]
        public string UserProfileId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the UserProfile associated to this comment.
        /// </summary>
        public virtual UserProfile? UserProfile { get; set; }

        /// <summary>
        /// Gets or sets the text of the comment.
        /// Text supports 1000 characters plus 344 for Encryption and Encoding overhead.
        /// </summary>
        [MaxLength(1344)]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the entry type code.
        /// The value is one of <see cref="Constants.CommentEntryType"/>.
        /// </summary>
        [MaxLength(3)]
        [Required]
        public string EntryTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the related event id.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string ParentEntryId { get; set; } = string.Empty;
    }
}
