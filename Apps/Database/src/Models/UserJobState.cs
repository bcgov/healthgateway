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
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Stores the current resumable state for a logical job.
    /// One row per job name.
    /// </summary>
    [Index(nameof(Hdid), nameof(JobName), IsUnique = true)]
    public class UserJobState : AuditableEntity
    {
        /// <summary>
        /// Gets the generated identifier.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the short unique job name.
        /// Example: NotificationBackfill.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string JobName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the HDID for this run.
        /// </summary>
        [Column("UserProfileId")]
        [MaxLength(52)]
        public string Hdid { get; init; } = null!;

        /// <summary>
        /// Gets or sets the processed date time for this run.
        /// </summary>
        public DateTime ProcessedDateTime { get; init; }

        /// <summary>
        /// Gets the user profile.
        /// </summary>
        public virtual UserProfile UserProfile { get; init; } = null!;
    }
}
