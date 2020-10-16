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
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;

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
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Note timeline datetime.
        /// </summary>
        public DateTime JournalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note db version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the datetime the entity was created.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string CreatedBy { get; set; } = null!;

        /// <summary>
        /// Gets or sets the datetime the entity was updated.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public DateTime UpdatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user/system that created the entity.
        /// This is generally set by the baseDbContext.
        /// </summary>
        public string UpdatedBy { get; set; } = null!;

        /// <summary>
        /// Constructs a UserNote model from a Node database model.
        /// </summary>
        /// <param name="model">The note database model.</param>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt note.</param>
        /// <param name="key">The security key.</param>
        /// <returns>The user note model.</returns>
        public static UserNote CreateFromDbModel(Database.Models.Note model, ICryptoDelegate cryptoDelegate, string key)
        {
            return new UserNote()
            {
                Id = model.Id,
                HdId = model.HdId,
                JournalDateTime = model.JournalDateTime,
                Version = model.Version,
                CreatedDateTime = model.CreatedDateTime,
                CreatedBy = model.CreatedBy,
                UpdatedDateTime = model.UpdatedDateTime,
                UpdatedBy = model.UpdatedBy,
                Title = model.Title != null ? cryptoDelegate.Decrypt(key, model.Title) : string.Empty,
                Text = model.Text != null ? cryptoDelegate.Decrypt(key, model.Text) : string.Empty,
            };
        }

        /// <summary>
        /// Constructs a List of UserNote models from a List of Node database models.
        /// </summary>
        /// <param name="models">The list of note database model.</param>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt note.</param>
        /// <param name="key">The security key.</param>
        /// <returns>A list of use notes.</returns>
        public static IEnumerable<UserNote> CreateListFromDbModel(IEnumerable<Database.Models.Note> models, ICryptoDelegate cryptoDelegate, string key)
        {
            List<UserNote> newList = new List<UserNote>();
            foreach (Database.Models.Note model in models)
            {
                newList.Add(UserNote.CreateFromDbModel(model, cryptoDelegate, key));
            }

            return newList;
        }

        /// <summary>
        /// Constructs a database Note model from a user Node model.
        /// </summary>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt note.</param>
        /// <param name="key">The security key.</param>
        /// <returns>The database user note model.</returns>
        public Database.Models.Note ToDbModel(ICryptoDelegate cryptoDelegate, string key)
        {
            return new Database.Models.Note()
            {
                Id = this.Id,
                HdId = this.HdId,
                JournalDateTime = this.JournalDateTime,
                Version = this.Version,
                CreatedDateTime = this.CreatedDateTime,
                CreatedBy = this.CreatedBy,
                UpdatedDateTime = this.UpdatedDateTime,
                UpdatedBy = this.UpdatedBy,
                Title = cryptoDelegate.Encrypt(key, this.Title),
                Text = cryptoDelegate.Encrypt(key, this.Text),
            };
        }
    }
}
