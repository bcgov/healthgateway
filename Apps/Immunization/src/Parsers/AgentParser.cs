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
namespace HealthGateway.Immunization.Parser
{
    using System.Collections.Generic;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;

    /// <summary>
    /// Provides parser methods for ImmunizationAgents.
    /// </summary>
    public static class AgentParser
    {
        /// <summary>
        /// Creates a Immunization Model object from a PHSA model.
        /// </summary>
        /// <param name="model">The immunization object to convert.</param>
        /// <returns>The newly created ImmunizationModel object.</returns>
        public static ImmunizationAgent FromPHSAModel(ImmunizationAgentResponse model)
        {
            return new ImmunizationAgent()
            {
                Code = model.Code,
                Name = model.Name,
                LotNumber = model.LotNumber,
                ProductName = model.ProductName,
            };
        }

        /// <summary>
        /// Creates a List of ImmunizationAgents object from a PHSA model.
        /// </summary>
        /// <param name="immunizationAgentResponse">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationAgent objects.</returns>
        public static IEnumerable<ImmunizationAgent> FromPHSAModelList(IEnumerable<ImmunizationAgentResponse>? immunizationAgentResponse)
        {
            List<ImmunizationAgent> immunizationAgents = new();
            if (immunizationAgentResponse != null)
            {
                foreach (ImmunizationAgentResponse immunizationAgentModel in immunizationAgentResponse)
                {
                    immunizationAgents.Add(AgentParser.FromPHSAModel(immunizationAgentModel));
                }
            }

            return immunizationAgents;
        }

        /// <summary>
        /// Creates a List of ImmunizationAgents object from a PHSA model.
        /// </summary>
        /// <param name="vaccineCodes">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationAgent objects.</returns>
        public static IEnumerable<ImmunizationAgent> FromPHSACodesModel(IEnumerable<SystemCode>? vaccineCodes)
        {
            List<ImmunizationAgent> immunizationAgents = new();
            foreach (SystemCode systemCode in vaccineCodes!)
            {
                immunizationAgents.Add(
                    new ImmunizationAgent()
                    {
                        Code = systemCode.Code,
                        Name = systemCode.Display,
                    });
            }

            return immunizationAgents;
        }
    }
}
