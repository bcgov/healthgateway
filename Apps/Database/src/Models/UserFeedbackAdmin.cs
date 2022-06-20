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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// An administrative view of UserFeedback with Email populated from UserProfile.
    /// </summary>
    public class UserFeedbackAdmin : UserFeedback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackAdmin"/> class.
        /// </summary>
        public UserFeedbackAdmin()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackAdmin"/> class.
        /// </summary>
        /// <param name="tags">The tag collection.</param>
        public UserFeedbackAdmin(ICollection<UserFeedbackTag> tags)
            : base(tags)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackAdmin"/> class.
        /// </summary>
        /// <param name="feedback">The database model.</param>
        public UserFeedbackAdmin(UserFeedback feedback)
            : base(feedback.Tags)
        {
            this.Id = feedback.Id;
            this.IsSatisfied = feedback.IsSatisfied;
            this.IsReviewed = feedback.IsReviewed;
            this.UpdatedBy = feedback.UpdatedBy;
            this.Comment = feedback.Comment;
            this.CreatedBy = feedback.CreatedBy;
            this.CreatedDateTime = feedback.CreatedDateTime;
            this.UserProfileId = feedback.UserProfileId;
            this.UpdatedDateTime = feedback.UpdatedDateTime;
            this.Version = feedback.Version;
        }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;
    }
}
