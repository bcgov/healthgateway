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
namespace HealthGateway.Common.Models.PHSA.Recommendation
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an Immunization Recommendation response.
    /// </summary>
    public class ImmunizationRecommendationResponse
    {
        /// <summary>
        /// Gets or sets the Forecast Creation Date.
        /// </summary>
        [JsonPropertyName("forecastCreationDate")]
        public DateOnly ForecastCreationDate { get; set; }

        /// <summary>
        /// Gets or sets the Recommendation Id.
        /// </summary>
        [JsonPropertyName("recommendationId")]
        public string RecommendationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Recommendation Source System.
        /// </summary>
        [JsonPropertyName("recommendationSourceSystem")]
        public string RecommendationSourceSystem { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Recommendation Source System Id.
        /// </summary>
        [JsonPropertyName("recommendationSourceSystemId")]
        public string RecommendationSourceSystemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of recommendation definitions.
        /// </summary>
        [JsonPropertyName("recommendations")]
        public IEnumerable<RecommendationResponse> Recommendations { get; set; } = [];
    }
}
