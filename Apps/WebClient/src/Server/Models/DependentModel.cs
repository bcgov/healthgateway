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
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Dependent Model.
    /// </summary>
    public class DependentModel
    {
        /// <summary>
        /// Gets or sets the dependent hdid.
        /// </summary>
        [JsonPropertyName("hdId")]
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent's hdid.
        /// </summary>
        [JsonPropertyName("parentHdId")]
        public string ParentHdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Patient.
        /// </summary>
        [JsonPropertyName("patient")]
        public Patient Patient { get; set; } = new Patient();

        /// <summary>
        /// Constructs a DependentModel from a Dependent database model.
        /// </summary>
        /// <param name="model">The dependent database model.</param>
        /// <returns>The user note model.</returns>
        public static DependentModel CreateFromDbModel(UserDelegate model)
        {
            return new DependentModel()
            {
                HdId = model.OwnerId,
                ParentHdId = model.DelegateId,
            };
        }

        /// <summary>
        /// Constructs a List of DependentModels from a List of Dependent database models.
        /// </summary>
        /// <param name="models">The list of Dependent database model.</param>
        /// <returns>A list of DependentModels.</returns>
        public static IEnumerable<DependentModel> CreateListFromDbModel(IEnumerable<Database.Models.UserDelegate> models)
        {
            List<DependentModel> newList = new List<DependentModel>();
            foreach (Database.Models.UserDelegate model in models)
            {
                newList.Add(DependentModel.CreateFromDbModel(model));
            }

            return newList;
        }

        /// <summary>
        /// Constructs a database Note model from a user Node model.
        /// </summary>
        /// <returns>The database Dependent model.</returns>
        public Database.Models.UserDelegate ToDbModel()
        {
            return new Database.Models.UserDelegate()
            {
                OwnerId = this.HdId,
                DelegateId = this.ParentHdId,
            };
        }
    }
}
