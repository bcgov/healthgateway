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
namespace HealthGateway.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a representation of an AdminTag.
    /// </summary>
    public class AdminTagView
    {
        /// <summary>
        /// Gets or sets the tag id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tag name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tag version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Converts a list of DB UserFeedbackTag into a list of AdminTagView.
        /// </summary>
        /// <param name="adminTags">The list of DB models.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IList<AdminTagView> FromDbFeedbackModelCollection(ICollection<UserFeedbackTag> adminTags)
        {
            List<AdminTagView> retList = new ();
            foreach (UserFeedbackTag tag in adminTags)
            {
                retList.Add(CreateFromDbFeedbackModel(tag));
            }

            return retList;
        }

        /// <summary>
        /// Constructs a AdminTagView from an UserFeedbackTag model.
        /// </summary>
        /// <param name="model">The database model.</param>
        /// <returns>A new AdminTagView.</returns>
        public static AdminTagView CreateFromDbFeedbackModel(UserFeedbackTag model)
        {
            return new AdminTagView()
            {
                Id = model.AdminTag.AdminTagId,
                Name = model.AdminTag.Name,
                Version = model.AdminTag.Version,
            };
        }

        /// <summary>
        /// Converts a list of DB AdminTag into a list of AdminTagView.
        /// </summary>
        /// <param name="adminTags">The list of DB models.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IList<AdminTagView> FromDbAdminTagModelCollection(IEnumerable<AdminTag> adminTags)
        {
            List<AdminTagView> retList = new ();
            foreach (AdminTag tag in adminTags)
            {
                retList.Add(CreateFromDbAdminTagModel(tag));
            }

            return retList;
        }

        /// <summary>
        /// Constructs a AdminTagView from an AdminTag model.
        /// </summary>
        /// <param name="model">The database model.</param>
        /// <returns>A new AdminTagView.</returns>
        public static AdminTagView CreateFromDbAdminTagModel(AdminTag model)
        {
            return new AdminTagView()
            {
                Id = model.AdminTagId,
                Name = model.Name,
                Version = model.Version,
            };
        }
    }
}
