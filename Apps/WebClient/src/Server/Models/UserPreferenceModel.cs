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
    using System.Linq;

    /// <summary>
    /// Model that provides a user representation of an user preference database model.
    /// </summary>
    public class UserPreferenceModel
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user dismissed my note popover.
        /// </summary>
        public bool DisableTutorialPopover { get; set; }

        /// <summary>
        /// Constructs a UserPreference model from a UserPreference database model.
        /// </summary>
        /// <param name="model">The user preference database model.</param>
        /// <returns>The user preference model.</returns>
        public static UserPreferenceModel CreateFromDbModel(IEnumerable<Database.Models.UserPreference> model)
        {
            if (model == null || !model.Any())
            {
                return null!;
            }

            IDictionary<string, string> dict = model.ToDictionary(k => k.Preference, v => v.Value);
            return new UserPreferenceModel()
            {
                HdId = model.FirstOrDefault().HdId,
                DisableTutorialPopover = bool.Parse(dict[nameof(DisableTutorialPopover)]),
            };
        }

        /// <summary>
        /// Constructs a database UserPreference model from a UserPreference model.
        /// </summary>
        /// <returns>The database user note model.</returns>
        public IEnumerable<Database.Models.UserPreference> ToDbModel()
        {
            List<Database.Models.UserPreference> model = new List<Database.Models.UserPreference>();
            model.Add(new Database.Models.UserPreference()
                {
                    HdId = this.HdId,
                    Preference = nameof(this.DisableTutorialPopover),
                    Value = this.DisableTutorialPopover.ToString(),
                });

            return model;
        }
    }
}
