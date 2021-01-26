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
namespace HealthGateway.Immunization.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Immunization.Models.PHSA;
    using HealthGateway.Immunization.Models.PHSA.Recommendation;

    /// <summary>
    /// The Immunization Agents model.
    /// </summary>
    public class ImmunizationAgent
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Lot Number.
        /// </summary>
        [JsonPropertyName("lotNumber")]
        public string LotNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunization Product Name.
        /// </summary>
        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

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
            List<ImmunizationAgent> immunizationAgents = new List<ImmunizationAgent>();
            if (immunizationAgentResponse != null)
            {
                foreach (ImmunizationAgentResponse immunizationAgentModel in immunizationAgentResponse)
                {
                    immunizationAgents.Add(ImmunizationAgent.FromPHSAModel(immunizationAgentModel));
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
            List<ImmunizationAgent> immunizationAgents = new List<ImmunizationAgent>();
            foreach (SystemCode systemCode in vaccineCodes)
            {
                immunizationAgents.Add(
                    new ImmunizationAgent()
                    {
                        Code = systemCode.Code,
                        Name = systemCode.Display
                    }
                );
            }

            return immunizationAgents;
        }


    }
}
