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
    using HealthGateway.Immunization.Models.PHSA;

    /// <summary>
    /// Represents a row in the Immunization Model.
    /// </summary>
    public class ImmunizationEvent
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Date of Immunization.
        /// </summary>
        [JsonPropertyName("dateOfImmunization")]
        public DateTime DateOfImmunization { get; set; }

        /// <summary>
        /// Gets or sets the Immunization Status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Provider or Clinic providing the Immunization.
        /// </summary>
        [JsonPropertyName("providerOrClinic")]
        public string ProviderOrClinic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization definition.
        /// </summary>
        [JsonPropertyName("immunization")]
        public ImmunizationDefinition Immunization { get; set; } = new ImmunizationDefinition();

        /// <summary>
        /// Gets or sets the Immunization forecast.
        /// </summary>
        [JsonPropertyName("forecast")]
        public ImmunizationForecast? Forecast { get; set; } = null;

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization view object to convert.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        public static ImmunizationEvent FromPHSAModel(ImmunizationViewResponse model)
        {
            return new ImmunizationEvent()
            {
                Id = model.SourceSystemId,
                DateOfImmunization = model.OccurrenceDateTime,
                ProviderOrClinic = model.ProviderOrClinic,
                Status = model.Status,
                Immunization = new ImmunizationDefinition() { Name = model.Name, ImmunizationAgents = ImmunizationAgent.FromPHSAModelList(model.ImmunizationAgents) },
                Forecast = model.ImmunizationForecast == null ? null : ImmunizationForecast.FromPHSAModel(model.ImmunizationForecast),
            };
        }

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="immunizationViewResponse">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationEvent objects.</returns>
        public static IEnumerable<ImmunizationEvent> FromPHSAModelList(IEnumerable<ImmunizationViewResponse>? immunizationViewResponse)
        {
            List<ImmunizationEvent> immunizations = new List<ImmunizationEvent>();
            if (immunizationViewResponse != null)
            {
                foreach (ImmunizationViewResponse immunizationViewModel in immunizationViewResponse)
                {
                    immunizations.Add(ImmunizationEvent.FromPHSAModel(immunizationViewModel));
                }
            }

            return immunizations;
        }
    }
}
