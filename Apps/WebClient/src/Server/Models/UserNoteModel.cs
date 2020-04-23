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
    /// Model that provides a user representation of an user profile database model.
    /// </summary>
    public class UserNote
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the Note timeline datetime.
        /// </summary>
        public DateTime JournalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note db version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Constructs a database Note model from a user Node model.
        /// Note: This does not add the title or text parameters.
        /// </summary>
        /// <returns>The database user note model.</returns>
        public Database.Models.Note ToDbModel()
        {
            return new Database.Models.Note()
            {
                Id = this.Id,
                HdId = this.HdId,
                JournalDateTime = this.JournalDateTime,
                Version = this.Version
            };
        }

        /// <summary>
        /// Constructs a UserNote model from a Node database model.
        /// Note: This does not add the title or text parameters.
        /// </summary>
        /// <param name="model">The note database model.</param>
        /// <returns>The user note model.</returns>
        public static UserNote CreateFromDbModel(Database.Models.Note model)
        {
            if (model == null)
            {
                return null!;
            }

            return new UserNote()
            {
                Id = model.Id,
                HdId = model.HdId,
                JournalDateTime = model.JournalDateTime,
                Version = model.Version
            };
        }
    }
}
