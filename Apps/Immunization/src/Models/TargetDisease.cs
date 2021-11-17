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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.PHSA.Recommendation;

    /// <summary>
    /// Represents a target disease.
    /// </summary>
    public class TargetDisease
    {
        /// <summary>
        /// Gets or sets the Disease Code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Disease Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Creates a list of ImmunizationRecommendation objects from a PHSA models.
        /// </summary>
        /// <param name="model">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationRecommendation objects.</returns>
        public static IList<TargetDisease> FromPHSAModelList(TargetDiseaseResponse model)
        {
            List<TargetDisease> targetDiseases = new();
            if (model != null)
            {
                foreach (SystemCode systemCode in model.TargetDiseaseCodes)
                {
                    targetDiseases.Add(FromPHSAModel(systemCode));
                }
            }

            return targetDiseases;
        }

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization view object to convert.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        private static TargetDisease FromPHSAModel(SystemCode model)
        {
            return new TargetDisease()
            {
                Code = model.Code,
                Name = model.Display,
            };
        }
    }
}
