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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a row in the Immunization Model.
    /// </summary>
    public class ImmunizationModel
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Date of Immunization.
        /// </summary>
        [JsonPropertyName("dateOfImmunization")]
        public DateTime DateOfImmunization { get; set; }

        /// <summary>
        /// Creates a Immunization Model object from a PHSA model.
        /// </summary>
        /// <param name="model">The medication result to convert.</param>
        /// <returns>The newly created medicationStatementHistory object.</returns>
        public static ImmunizationModel FromPHSAModel(ImmunizationResponse model)
        {
            return new ImmunizationModel()
            {
                Id = model.Id,
                Name = model.Name,
                DateOfImmunization = model.OccurrenceDateTime
            };
        }

        /// <summary>
        /// Creates a Immunization Model object from a PHSA model.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationModel objects.</returns>
        public static IEnumerable<ImmunizationModel> FromPHSAModelList(IEnumerable<ImmunizationResponse>? models)
        {
            List<ImmunizationModel> objects = new List<ImmunizationModel>();
            if (models != null)
            {
                foreach (ImmunizationResponse immunizationModel in models)
                {
                    objects.Add(ImmunizationModel.FromPHSAModel(immunizationModel));
                }
            }

            return objects;
        }
    }
}
