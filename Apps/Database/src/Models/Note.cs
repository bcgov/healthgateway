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
    /// A user entered Note.
    /// </summary>
    public class Note : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("NoteId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        [Key]
        [Column("UserProfileId")]
        [MaxLength(52)]
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the title.
        /// Title supports 100 characters plus 52 for Encryption and Encoding overhead.
        /// </summary>
        [MaxLength(152)]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the text of the note.
        /// Text supports 1000 characters plus 344 for Encryption and Encoding overhead.
        /// </summary>
        [MaxLength(1344)]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the Note timeline datetime.
        /// </summary>
        [Required]
        public DateTime JournalDateTime { get; set; }
    }
}
