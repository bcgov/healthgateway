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
namespace HealthGateway.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of a user feedback.
    /// </summary>
    public class UserFeedbackView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackView"/> class.
        /// </summary>
        public UserFeedbackView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackView"/> class.
        /// </summary>
        /// <param name="tags">The list of user feedback tags.</param>
        [JsonConstructor]
        public UserFeedbackView(IList<UserFeedbackTagView> tags)
        {
            this.Tags = tags;
        }

        /// <summary>
        /// Gets or sets the user feedback id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the related user profile id.
        /// </summary>
        public string? UserProfileId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the feedback comments.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is satisfied or not.
        /// </summary>
        public bool IsSatisfied { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the feedback is reviewed or not.
        /// </summary>
        public bool IsReviewed { get; set; }

        /// <summary>
        /// Gets or sets the date when the feedback was created.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the version of the resource.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the email if known for this feedback.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets the feedback admin tags.
        /// </summary>
        public IList<UserFeedbackTagView> Tags { get; } = new List<UserFeedbackTagView>();

        /// <summary>
        /// Constructs a UserFeedbackView from a UserFeedback model.
        /// </summary>
        /// <param name="model">A user feedback request models.</param>
        /// <returns>A new UserFeedbackView.</returns>
        public static UserFeedbackView CreateFromDbModel(UserFeedback model)
        {
            UserFeedbackView userFeedbackView = new()
            {
                Id = model.Id,
                UserProfileId = model.UserProfileId,
                Comment = model.Comment,
                CreatedDateTime = model.CreatedDateTime,
                IsReviewed = model.IsReviewed,
                IsSatisfied = model.IsSatisfied,
                Version = model.Version,
            };

            IList<UserFeedbackTagView> viewTags = UserFeedbackTagView.FromDbModelCollection(model.Tags);

            foreach (UserFeedbackTagView viewTag in viewTags)
            {
                userFeedbackView.Tags.Add(viewTag);
            }

            return userFeedbackView;
        }

        /// <summary>
        /// Constructs a List of UserFeedbackView from a list of UserFeedback models.
        /// </summary>
        /// <param name="models">List of user feedback models.</param>
        /// <returns>A list of UserFeedbackView.</returns>
        public static IList<UserFeedbackView> CreateListFromDbModel(IList<UserFeedback> models)
        {
            IList<UserFeedbackView> newList = new List<UserFeedbackView>();
            foreach (UserFeedback model in models)
            {
                newList.Add(CreateFromDbModel(model));
            }

            return newList;
        }

        /// <summary>
        /// Converts this view model into a DB model object.
        /// </summary>
        /// <returns>The DB model object.</returns>
        public UserFeedback ToDbModel()
        {
            return new UserFeedback()
            {
                Id = this.Id,
                Comment = this.Comment,
                CreatedDateTime = this.CreatedDateTime,
                IsReviewed = this.IsReviewed,
                IsSatisfied = this.IsSatisfied,
                Version = this.Version,
            };
        }
    }
}
