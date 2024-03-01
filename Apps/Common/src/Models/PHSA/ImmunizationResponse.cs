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
namespace HealthGateway.Common.Models.PHSA
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.PHSA.Recommendation;

    /// <summary>
    /// Represents Immunization Response.
    /// </summary>
    public class ImmunizationResponse
    {
        /// <summary>
        /// Gets or sets the collection of Immunization Views.
        /// </summary>
        [JsonPropertyName("immunizationViews")]
        public IEnumerable<ImmunizationViewResponse> ImmunizationViews { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of Immunization Recommendations.
        /// </summary>
        [JsonPropertyName("immunizationRecommendations")]
        public IEnumerable<ImmunizationRecommendationResponse> Recommendations { get; set; } = [];
    }
}
