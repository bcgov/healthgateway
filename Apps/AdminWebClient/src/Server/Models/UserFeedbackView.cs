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
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of a user feedback.
    /// </summary>
    public class UserFeedbackView
    {
        /// <summary>
        /// Gets or sets the user feedback id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the feedback comments.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the user is satisfied or not.
        /// </summary>
        public bool IsSatisfied { get; set; } = false;

        /// <summary>
        /// Gets or sets the value indicating whether the feedback is reviewed or not.
        /// </summary>
        public bool IsReviewed { get; set; } = false;

        /// <summary>
        /// Gets or sets the date when the feedback was created.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the version of the resource.
        /// </summary>
        public uint Version { get; set; } = 0;

        public UserFeedback ToDbModel()
        {
            return new UserFeedback()
            {
                Id = this.Id,
                Comment = this.Comment,
                CreatedDateTime = this.CreatedDateTime,
                IsReviewed = this.IsReviewed,
                IsSatisfied = this.IsSatisfied,
                Version = this.Version
            };
        }

        /// <summary>
        /// Constructs a UserFeedbackView from a UserFeedback model.
        /// </summary>
        /// <param name="model">A user feedback request models.</param>
        /// <returns>A new UserFeedbackView.</returns>
        public static UserFeedbackView CreateFromDbModel(UserFeedback model)
        {
            return new UserFeedbackView()
            {
                Id = model.Id,
                Comment = model.Comment,
                CreatedDateTime = model.CreatedDateTime,
                IsReviewed = model.IsReviewed,
                IsSatisfied = model.IsSatisfied,
                Version = model.Version
            };
        }

        /// <summary>
        /// Constructs a List of UserFeedbackView from a list of UserFeedback models.
        /// </summary>
        /// <param name="models">List of user feedback models.</param>
        /// <returns>A list of UserFeedbackView.</returns>
        public static List<UserFeedbackView> CreateListFromDbModel(List<UserFeedback> models)
        {
            List<UserFeedbackView> newList = new List<UserFeedbackView>();
            foreach (UserFeedback model in models)
            {
                newList.Add(UserFeedbackView.CreateFromDbModel(model));
            }
            return newList;
        }

        /// <summary>
        /// Constructs a List of UserFeedbackView from a list of UserFeedback models.
        /// </summary>
        /// <param name="view">List of user feedback models.</param>
        /// <returns>A list of UserFeedbackView.</returns>

    }
}
