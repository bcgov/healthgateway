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
namespace HealthGateway.Common.Models.PHSA.Recommendation
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA Target Disease data model.
    /// </summary>
    public class TargetDiseaseResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDiseaseResponse"/> class.
        /// </summary>
        public TargetDiseaseResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetDiseaseResponse"/> class.
        /// </summary>
        /// <param name="targetDiseaseCodes">The initialized list of target disease codes.</param>
        [JsonConstructor]
        public TargetDiseaseResponse(IList<SystemCode> targetDiseaseCodes)
        {
            this.TargetDiseaseCodes = targetDiseaseCodes;
        }

        /// <summary>
        /// Gets the Target Disese Codes.
        /// </summary>
        [JsonPropertyName("targetDiseaseCodes")]
        public IList<SystemCode> TargetDiseaseCodes { get; } = new List<SystemCode>();
    }
}
