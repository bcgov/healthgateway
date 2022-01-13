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
        /// Converts a list of DB AdminTag into a list of AdminTagView.
        /// </summary>
        /// <param name="adminTags">The list of DB models.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IList<AdminTagView> FromDbModelCollection(IEnumerable<AdminTag> adminTags)
        {
            List<AdminTagView> retList = new();
            foreach (AdminTag tag in adminTags)
            {
                retList.Add(FromDbModel(tag));
            }

            return retList;
        }

        /// <summary>
        /// Constructs a AdminTagView from an AdminTag model.
        /// </summary>
        /// <param name="model">The database model.</param>
        /// <returns>A new AdminTagView.</returns>
        public static AdminTagView FromDbModel(AdminTag model)
        {
            return new AdminTagView()
            {
                Id = model.AdminTagId,
                Name = model.Name,
                Version = model.Version,
            };
        }

        /// <summary>
        /// Constructs an AdminTag from the object.
        /// </summary>
        /// <returns>A new AdminTag.</returns>
        public AdminTag ToDbModel()
        {
            return new AdminTag()
            {
                AdminTagId = this.Id,
                Name = this.Name,
                Version = this.Version,
            };
        }
    }
}
