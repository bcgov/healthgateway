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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.Immunization;

    /// <summary>
    /// Represents an Immunization Recommendation.
    /// </summary>
    public class ImmunizationRecommendation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationRecommendation"/> class.
        /// </summary>
        /// <param name="targetDiseases">The list of target diseases.</param>
        [JsonConstructor]
        public ImmunizationRecommendation(IList<TargetDisease> targetDiseases)
        {
            this.TargetDiseases = targetDiseases;
        }

        /// <summary>
        /// Gets or sets the Recommendation Set Id.
        /// </summary>
        [JsonPropertyName("recommendationSetId")]
        public string RecommendationSetId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Disease Eligible Date.
        /// </summary>
        [JsonPropertyName("diseaseEligibleDate")]
        public DateTime? DiseaseEligibleDate { get; set; }

        /// <summary>
        /// Gets or sets the Disease Due Date.
        /// </summary>
        [JsonPropertyName("diseaseDueDate")]
        public DateTime? DiseaseDueDate { get; set; }

        /// <summary>
        /// Gets or sets the Agent Eligible Date.
        /// </summary>
        [JsonPropertyName("agentEligibleDate")]
        public DateTime? AgentEligibleDate { get; set; }

        /// <summary>
        /// Gets or sets the Agent Due Date.
        /// </summary>
        [JsonPropertyName("agentDueDate")]
        public DateTime? AgentDueDate { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets the Target Diseases.
        /// </summary>
        [JsonPropertyName("targetDiseases")]
        public IList<TargetDisease> TargetDiseases { get; }

        /// <summary>
        /// Gets or sets the Immunization definition.
        /// </summary>
        [JsonPropertyName("immunization")]
        public ImmunizationDefinition Immunization { get; set; } = new();
    }
}
