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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Represents a Dependent Model.
    /// </summary>
    public class DependentModel
    {
        /// <summary>
        /// Gets or sets the dependent hdid.
        /// </summary>
        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent's hdid.
        /// </summary>
        [JsonPropertyName("delegateId")]
        public string DelegateId { get; set; } = string.Empty;

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
                OwnerId = model.OwnerId,
                DelegateId = model.DelegateId,
            };
        }

        /// <summary>
        /// Constructs a database Note model from a user Node model.
        /// </summary>
        /// <returns>The database Dependent model.</returns>
        public Database.Models.UserDelegate ToDbModel()
        {
            return new Database.Models.UserDelegate()
            {
                OwnerId = this.OwnerId,
                DelegateId = this.DelegateId,
            };
        }
    }
}
