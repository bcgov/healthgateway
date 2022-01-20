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
namespace HealthGateway.Admin.Server.Models
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a representation of UserFeedbackTagView.
    /// </summary>
    public class UserFeedbackTagView
    {
        /// <summary>
        /// Gets or sets the user feedback tag id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the admin tag.
        /// </summary>
        public AdminTagView Tag { get; set; } = new AdminTagView();

        /// <summary>
        /// Gets or sets the tag version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Converts a list of DB UserFeedbackTag into a list of UserFeedbackTagView.
        /// </summary>
        /// <param name="adminTags">The list of DB models.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IList<UserFeedbackTagView> FromDbModelCollection(ICollection<UserFeedbackTag> adminTags)
        {
            List<UserFeedbackTagView> retList = new();
            foreach (UserFeedbackTag tag in adminTags)
            {
                retList.Add(FromDbModel(tag));
            }

            return retList;
        }

        /// <summary>
        /// Constructs a UserFeedbackTagView from an UserFeedbackTag model.
        /// </summary>
        /// <param name="model">The database model.</param>
        /// <returns>A new UserFeedbackTagView.</returns>
        public static UserFeedbackTagView FromDbModel(UserFeedbackTag model)
        {
            UserFeedbackTagView newDBModel = new()
            {
                Id = model.UserFeedbackTagId,
                Version = model.Version,
            };

            if (model.AdminTag != null)
            {
                newDBModel.Tag = AdminTagView.FromDbModel(model.AdminTag);
            }

            return newDBModel;
        }

        /// <summary>
        /// Returns a new UserFeedbackTag from the object.
        /// </summary>
        /// <returns>A new UserFeedbackTag.</returns>
        public UserFeedbackTag ToDbModel()
        {
            return new UserFeedbackTag()
            {
                UserFeedbackTagId = this.Id,
                Version = this.Version,
            };
        }
    }
}
