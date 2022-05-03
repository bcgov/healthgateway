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
namespace HealthGateway.GatewayApi.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Model that provides a user preference.
    /// </summary>
    public class UserPreferenceModel
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Preference.
        /// </summary>
        public string Preference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

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
        /// Constructs a UserPreferenceModel model from a UserPreference database model.
        /// </summary>
        /// <param name="model">The user preference database model.</param>
        /// <returns>The user comment model.</returns>
        public static UserPreferenceModel CreateFromDbModel(Database.Models.UserPreference model)
        {
            return new UserPreferenceModel()
            {
                HdId = model.HdId,
                Preference = model.Preference,
                Value = model.Value,
                Version = model.Version,
                CreatedDateTime = model.CreatedDateTime,
                CreatedBy = model.CreatedBy,
                UpdatedDateTime = model.UpdatedDateTime,
                UpdatedBy = model.UpdatedBy,
            };
        }

        /// <summary>
        /// Constructs a List of UserPreference models from a List of User Preference database models.
        /// </summary>
        /// <param name="models">The list of user preference database model.</param>
        /// <returns>A list of use comments.</returns>
        public static IEnumerable<UserPreferenceModel> CreateListFromDbModel(IEnumerable<Database.Models.UserPreference> models)
        {
            List<UserPreferenceModel> newList = new List<UserPreferenceModel>();
            foreach (Database.Models.UserPreference model in models)
            {
                newList.Add(UserPreferenceModel.CreateFromDbModel(model));
            }

            return newList;
        }

        /// <summary>
        /// Constructs a database comment model from a user preference model.
        /// </summary>
        /// <returns>The database user preference model.</returns>
        public Database.Models.UserPreference ToDbModel()
        {
            return new Database.Models.UserPreference()
            {
                HdId = this.HdId,
                Preference = this.Preference,
                Value = this.Value,
                Version = this.Version,
                CreatedDateTime = this.CreatedDateTime,
                CreatedBy = this.CreatedBy,
                UpdatedDateTime = this.UpdatedDateTime,
                UpdatedBy = this.UpdatedBy,
            };
        }
    }
}
