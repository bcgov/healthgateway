﻿// -------------------------------------------------------------------------
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
    /// A generic mechanism to log events for later review.
    /// </summary>
    public class EventLog : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("EventLogId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Event Name
        /// Text supports 1000 characters plus 344 for Encryption and Encoding overhead.
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the event source.
        /// </summary>
        [MaxLength(300)]
        [Required]
        public string EventSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        [MaxLength(1000)]
        [Required]
        public string EventDescription { get; set; } = string.Empty;
    }
}
