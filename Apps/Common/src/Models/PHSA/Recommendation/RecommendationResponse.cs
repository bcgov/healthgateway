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
    /// The PHSA Immunization Recommendation data model.
    /// </summary>
    public class RecommendationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationResponse"/> class.
        /// </summary>
        public RecommendationResponse()
        {
            this.DateCriterions = new List<DateCriterion>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationResponse"/> class.
        /// </summary>
        /// <param name="dateCriterions">The initialized list of dateCriterions.</param>
        [JsonConstructor]
        public RecommendationResponse(IList<DateCriterion> dateCriterions)
        {
            this.DateCriterions = dateCriterions;
        }

        /// <summary>
        /// Gets the Date Criterion.
        /// </summary>
        [JsonPropertyName("dateCriterion")]
        public IList<DateCriterion> DateCriterions { get; }

        /// <summary>
        /// Gets or sets the Forecast Status.
        /// </summary>
        [JsonPropertyName("forecastStatus")]
        public ForecastStatusModel ForecastStatus { get; set; } = new();

        /// <summary>
        /// Gets or sets the Target Disease.
        /// </summary>
        [JsonPropertyName("targetDisease")]
        public TargetDiseaseResponse TargetDisease { get; set; } = new();

        /// <summary>
        /// Gets or sets the Vaccine Code.
        /// </summary>
        [JsonPropertyName("vaccineCode")]
        public VaccineCode VaccineCode { get; set; } = new();
    }
}
