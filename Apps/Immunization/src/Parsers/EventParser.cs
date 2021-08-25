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
namespace HealthGateway.Immunization.Parser
{
    using System.Collections.Generic;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Provides parsing methods for Immunization Events.
    /// </summary>
    public static class EventParser
    {
        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization view object to convert.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        public static ImmunizationEvent FromPHSAModel(ImmunizationViewResponse model)
        {
            return new ImmunizationEvent()
            {
                Id = model.Id.ToString(),
                DateOfImmunization = model.OccurrenceDateTime,
                TargetedDisease = model.TargetedDisease,
                ProviderOrClinic = model.ProviderOrClinic,
                Status = model.Status,
                Immunization = new ImmunizationDefinition() { Name = model.Name, ImmunizationAgents = AgentParser.FromPHSAModelList(model.ImmunizationAgents) },
                Forecast = model.ImmunizationForecast == null ? null : ForecastParser.FromPHSAModel(model.ImmunizationForecast),
            };
        }

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="immunizationViewResponse">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationEvent objects.</returns>
        public static IList<ImmunizationEvent> FromPHSAModelList(IEnumerable<ImmunizationViewResponse>? immunizationViewResponse)
        {
            List<ImmunizationEvent> immunizations = new List<ImmunizationEvent>();
            if (immunizationViewResponse != null)
            {
                foreach (ImmunizationViewResponse immunizationViewModel in immunizationViewResponse)
                {
                    immunizations.Add(EventParser.FromPHSAModel(immunizationViewModel));
                }
            }

            return immunizations;
        }
    }
}
