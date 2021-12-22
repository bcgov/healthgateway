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
    using System.Text.Json;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// An asbtract class representing a data store for JSON cacheable objects.
    /// </summary>
    public class GenericCache : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Column("GenericCacheId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Users HdId.
        /// </summary>
        [Required]
        [MaxLength(54)]
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cache domain.
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cache expiry datetime.
        /// </summary>
        [Required]
        public DateTime? ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the JSON Document type.
        /// This value is used by code to reconstruct the JSON POCO.
        /// </summary>
        [Required]
        public string? JSONType { get; set; }

        /// <summary>
        /// Gets or sets the JSONDocument to store.
        /// </summary>
        [Required]
        public JsonDocument? JSON { get; set; }
    }
}
