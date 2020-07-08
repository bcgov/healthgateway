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
namespace HealthGateway.WebClient.Models
{
    using System;

    /// <summary>
    /// Model that provides a user representation of an user preference database model.
    /// </summary>
    public class UserPreferenceModel
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user dismissed my note popover.
        /// </summary>
        public bool DismissedMyNotePopover { get; set; }

        /// <summary>
        /// Constructs a UserPreference model from a UserPreference database model.
        /// </summary>
        /// <param name="model">The user preference database model.</param>
        /// <returns>The user preference model.</returns>
        public static UserPreferenceModel CreateFromDbModel(Database.Models.UserPreference model)
        {
            if (model == null)
            {
                return null!;
            }

            return new UserPreferenceModel()
            {
                Id = model.Id,
                HdId = model.HdId,
                DismissedMyNotePopover = model.DismissedMyNotePopover,
            };
        }

        /// <summary>
        /// Constructs a database UserPreference model from a UserPreference model.
        /// </summary>
        /// <returns>The database user note model.</returns>
        public Database.Models.UserPreference ToDbModel()
        {
            return new Database.Models.UserPreference()
            {
                Id = this.Id,
                HdId = this.HdId,
                DismissedMyNotePopover = this.DismissedMyNotePopover,
            };
        }
    }
}
