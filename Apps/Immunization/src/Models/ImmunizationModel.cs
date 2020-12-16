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
        /// Gets or sets the Provider or Clinic providing the Immunization.
        /// </summary>
        [JsonPropertyName("providerOrClinic")]
        public string ProviderOrClinic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Agents.
        /// </summary>
        public IEnumerable<ImmunizationAgentsResponse> ImmunizationAgents { get; set; } = new List<ImmunizationAgentsResponse>();

        /// <summary>
        /// Creates a Immunization Model object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization object to convert.</param>
        /// <returns>The newly created ImmunizationModel object.</returns>
        public static ImmunizationModel FromPHSAModel(ImmunizationResponse model)
        {
            return new ImmunizationModel()
            {
                Id = model.SourceSystemId,
                Name = model.Name,
                DateOfImmunization = model.OccurrenceDateTime,
                ProviderOrClinic = model.ProviderOrClinic,
                ImmunizationAgents = model.ImmunizationAgents,
            };
        }

        /// <summary>
        /// Creates a Immunization Model object from a PHSA model.
        /// </summary>
        /// <param name="immunizationResponse">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationModel objects.</returns>
        public static IEnumerable<ImmunizationModel> FromPHSAModelList(IEnumerable<ImmunizationResponse>? immunizationResponse)
        {
            List<ImmunizationModel> immunizations = new List<ImmunizationModel>();
            if (immunizationResponse != null)
            {
                foreach (ImmunizationResponse immunizationModel in immunizationResponse)
                {
                    immunizations.Add(ImmunizationModel.FromPHSAModel(immunizationModel));
                }
            }

            return immunizations;
        }
    }
}
