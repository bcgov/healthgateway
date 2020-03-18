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
    ///  A data store for cacheable objects.
    /// </summary>
    /// <typeparam name="T">The generic type to cache.</typeparam>
    public class GenericCache<T> : AuditableEntity
        where T : class
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Column("GenericCacheId")]
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Users HdId.
        /// </summary>
        [Required]
        [MaxLength(54)]
        public string? HdId { get; set; }

        /// <summary>
        /// Gets or sets the Timeline datetime.
        /// </summary>
        [Required]
        public DateTime? TimelineDateTime { get; set; }

        /// <summary>
        /// Gets or sets serialized JSON object data.
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public T ObjectJSON { get; set; } = default!;

        /// <summary>
        /// Gets or sets the cache expiry datetime.
        /// </summary>
        [Required]
        public DateTime? ExpiryDateTime { get; set; }
    }
}
