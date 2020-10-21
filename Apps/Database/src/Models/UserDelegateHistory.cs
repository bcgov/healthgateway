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
    /// The user delegate history model.
    /// </summary>
    public class UserDelegateHistory : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Column("UserDelegateHistoryId")]
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the owner of the hdid.
        /// </summary>
        [Required]
        [MaxLength(52)]
        public string OwnerId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the hdid which has delegated access to the owner Id.
        /// </summary>
        [Required]
        [MaxLength(52)]
        public string DelegateId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the operation that created this history row.
        /// </summary>
        [Required]
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the datetime the operation was performed.
        /// </summary>
        [Required]
        public DateTime OperationDateTime { get; set; } = DateTime.MaxValue;    
    }
}
