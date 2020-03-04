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
    /// Model that provides a user representation of an patient note database model.
    /// </summary>
    public class NoteModel
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the note text.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the note title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the entry date.
        /// </summary>
        public DateTime JournalDateTime { get; set; }

        /// <summary>
        /// Constructs a UserNote model from a UserNote database model.
        /// </summary>
        /// <param name="model">The note database model.</param>
        /// <returns>The note model.</returns>
        public static NoteModel CreateFromDbModel(Database.Models.Note model)
        {
            if (model == null)
            {
                return null!;
            }

            return new NoteModel()
            {
                Id = model.Id,
                HdId = model.HdId,
                JournalDateTime = model.JournalDateTime,
                Text = model.Text,
                Title = model.Title,
            };
        }
    }
}
