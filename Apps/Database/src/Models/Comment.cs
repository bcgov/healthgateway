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
        [Column("UserProfileId")]
        [MaxLength(52)]
        [Required]
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        [MaxLength(1000)]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the event type code.
        /// The value is one of <see cref="Constant.EventType"/>.
        /// </summary>
        [MaxLength(3)]
        [Required]
        public string EventTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the related event id.
        /// </summary>
        [MaxLength(32)]
        [Required]
        public string EventId { get; set; } = string.Empty;
    }
}
