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
namespace HealthGateway.Immunization.Models.PHSA.Recommendation
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA Vaccine Code data model.
    /// </summary>
    public class VaccineCode
    {
        /// <summary>
        /// Gets or sets the Vaccine Code Text.
        /// </summary>
        [JsonPropertyName("vaccineCodeText")]
        public string VaccineCodeText { get; set; } = string.Empty;

        /// <summary>
        /// Gets the Vaccine codes.
        /// </summary>
        [JsonPropertyName("vaccineCodes")]
        public IList<SystemCode> VaccineCodes { get; } = new List<SystemCode>();
    }
}
