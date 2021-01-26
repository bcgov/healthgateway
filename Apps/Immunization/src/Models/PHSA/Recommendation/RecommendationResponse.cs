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
    /// The PHSA Immunization Recommendation data model.
    /// </summary>
    public class RecommendationResponse
    {
        /// <summary>
        /// Gets or sets the Date Criterion.
        /// </summary>
        [JsonPropertyName("dateCriterion")]
        public IList<DateCriterion> DateCriterions { get; set; } = new List<DateCriterion>();

        /// <summary>
        /// Gets or sets the Forecast Status.
        /// </summary>
        [JsonPropertyName("forecastStatus")]
        public ForecastStatusModel ForecastStatus { get; set; } = new ForecastStatusModel();

        /// <summary>
        /// Gets or sets the Target Disease.
        /// </summary>
        [JsonPropertyName("targetDisease")]
        public TargetDiseaseResponse TargetDisease { get; set; } = new TargetDiseaseResponse();

        /// <summary>
        /// Gets or sets the Vaccine Code.
        /// </summary>
        [JsonPropertyName("vaccineCode")]
        public VaccineCode VaccineCode { get; set; } = new VaccineCode();
    }
}
