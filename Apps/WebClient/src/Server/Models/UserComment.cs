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
    public class UserComment
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string UserProfileId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the text of the comment.
        /// Text supports 1000 characters plus 344 for Encryption and Encoding overhead.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry type code.
        /// The value is one of <see cref="HealthGateway.Database.Constants.CommentEntryType"/>.
        /// </summary>
        public string EntryTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the related event id.
        /// </summary>
        public string ParentEntryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the comment db version.
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
        /// Constructs a UserComment model from a Node database model.
        /// </summary>
        /// <param name="model">The comment database model.</param>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt comment.</param>
        /// <param name="key">The security key.</param>
        /// <returns>The user comment model.</returns>
        public static UserComment CreateFromDbModel(Database.Models.Comment model, ICryptoDelegate cryptoDelegate, string key)
        {
            return new UserComment()
            {
                Id = model.Id,
                UserProfileId = model.UserProfileId,
                EntryTypeCode = model.EntryTypeCode,
                ParentEntryId = model.ParentEntryId,
                Version = model.Version,
                CreatedDateTime = model.CreatedDateTime,
                CreatedBy = model.CreatedBy,
                UpdatedDateTime = model.UpdatedDateTime,
                UpdatedBy = model.UpdatedBy,
                Text = model.Text != null ? cryptoDelegate.Decrypt(key, model.Text) : string.Empty,
            };
        }

        /// <summary>
        /// Constructs a List of UserComment models from a List of Node database models.
        /// </summary>
        /// <param name="models">The list of comment database model.</param>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt comment.</param>
        /// <param name="key">The security key.</param>
        /// <returns>A list of use comments.</returns>
        public static IEnumerable<UserComment> CreateListFromDbModel(IEnumerable<Database.Models.Comment> models, ICryptoDelegate cryptoDelegate, string key)
        {
            List<UserComment> newList = new List<UserComment>();
            foreach (Database.Models.Comment model in models)
            {
                newList.Add(UserComment.CreateFromDbModel(model, cryptoDelegate, key));
            }

            return newList;
        }

        /// <summary>
        /// Constructs a database comment model from a user Node model.
        /// </summary>
        /// <param name="cryptoDelegate">Crypto delegate to decrypt comment.</param>
        /// <param name="key">The security key.</param>
        /// <returns>The database user comment model.</returns>
        public Database.Models.Comment ToDbModel(ICryptoDelegate cryptoDelegate, string key)
        {
            return new Database.Models.Comment()
            {
                Id = this.Id,
                UserProfileId = this.UserProfileId,
                EntryTypeCode = this.EntryTypeCode,
                ParentEntryId = this.ParentEntryId,
                Version = this.Version,
                CreatedDateTime = this.CreatedDateTime,
                CreatedBy = this.CreatedBy,
                UpdatedDateTime = this.UpdatedDateTime,
                UpdatedBy = this.UpdatedBy,
                Text = cryptoDelegate.Encrypt(key, this.Text),
            };
        }
    }
}
