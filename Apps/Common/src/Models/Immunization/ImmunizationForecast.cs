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
namespace HealthGateway.Common.Models.Immunization
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents Immunization Forecast.
    /// </summary>
    public class ImmunizationForecast
    {
        /// <summary>
        /// Gets or sets the Recommendation Id.
        /// </summary>
        [JsonPropertyName("recommendationId")]
        public string RecommendationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Create Date.
        /// </summary>
        [JsonPropertyName("createDate")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Display Name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Eligible Date.
        /// </summary>
        [JsonPropertyName("eligibleDate")]
        public DateTime EligibleDate { get; set; }

        /// <summary>
        /// Gets or sets the Due Date.
        /// </summary>
        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }
    }
}
