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
namespace HealthGateway.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The user feedback related tags.
    /// </summary>
    public class UserFeedbackTag : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid UserFeedbackTagId { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Required]
        [ForeignKey("AdminTagId")]
        public Guid AdminTagId { get; set; }

        /// <summary>
        /// Gets or sets the related user feedback id.
        /// </summary>
        [Required]
        [ForeignKey("UserFeedbackId")]
        public Guid UserFeedbackId { get; set; }

        /// <summary>
        /// Gets or sets the related user feedback model.
        /// </summary>
        public virtual UserFeedback? UserFeedback { get; set; }

        /// <summary>
        /// Gets or sets the related admin tag model.
        /// </summary>
        public virtual AdminTag? AdminTag { get; set; }
    }
}
