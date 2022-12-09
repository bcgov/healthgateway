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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The user feedback database model.
    /// </summary>
    public class UserFeedback : AuditableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedback"/> class.
        /// </summary>
        public UserFeedback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedback"/> class.
        /// </summary>
        /// <param name="tags">The tag collection.</param>
        public UserFeedback(ICollection<UserFeedbackTag> tags)
        {
            this.Tags = tags;
        }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        [Key]
        [Column("UserFeedbackId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feedback is satisfied or not.
        /// </summary>
        public bool IsSatisfied { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feedback was reviewed or not.
        /// </summary>
        public bool IsReviewed { get; set; }

        /// <summary>
        /// Gets or sets the feedback comment.
        /// </summary>
        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets the related user profile id.
        /// </summary>
        [MaxLength(52)]
        public string? UserProfileId { get; set; }

        /// <summary>
        /// Gets or sets the UserProfile associated to this user feedback.
        /// </summary>
        public virtual UserProfile? UserProfile { get; set; }

        /// <summary>
        /// Gets the related list of tags.
        /// </summary>
        public ICollection<UserFeedbackTag> Tags { get; } = new List<UserFeedbackTag>();
    }
}
