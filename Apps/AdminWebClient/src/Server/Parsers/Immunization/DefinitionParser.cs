// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Parsers.Immunization
{
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA.Recommendation;

    /// <summary>
    /// Provides parsing methods for Immunization definitions.
    /// </summary>
    public static class DefinitionParser
    {
        /// <summary>
        /// Creates an ImmunizationDefinition object from a PHSA model.
        /// </summary>
        /// <param name="model">The vaccine code object to convert.</param>
        /// <returns>The newly created ImmunizationDefinition object.</returns>
        public static ImmunizationDefinition FromPHSAModel(VaccineCode model)
        {
            return new ImmunizationDefinition()
            {
                Name = model.VaccineCodeText,
                ImmunizationAgents = AgentParser.FromPHSACodesModel(model.VaccineCodes),
            };
        }
    }
}
