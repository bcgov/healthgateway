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
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA Date Criterion data model.
    /// </summary>
    public class DateCriterion
    {
        /// <summary>
        /// Gets or sets the Date Criterion codes.
        /// </summary>
        [JsonPropertyName("dateCriterionCode")]
        public DateCriterionCode DateCriterionCode { get; set; } = new();

        /// <summary>
        /// Gets or sets the Date Criterion value.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
