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
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Immunization.Constants;
    using HealthGateway.Immunization.Models.PHSA.Recommendation;

    /// <summary>
    /// Represents an Immunization Recommendation.
    /// </summary>
    public class ImmunizationRecommendation
    {
        /// <summary>
        /// Gets or sets the Recommendation Set Id.
        /// </summary>
        [JsonPropertyName("recommendationSetId")]
        public string RecommendationSetId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Dissease Eligible Date.
        /// </summary>
        [JsonPropertyName("disseaseEligibleDate")]
        public DateTime? DisseaseEligibleDate { get; set; }

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
        public ForecastStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Target Diseases.
        /// </summary>
        [JsonPropertyName("targetDiseases")]
        public IList<TargetDisease> TargetDiseases { get; set; } = new List<TargetDisease>();

        /// <summary>
        /// Gets or sets the Immunization definition.
        /// </summary>
        [JsonPropertyName("immunization")]
        public ImmunizationDefinition Immunization { get; set; } = new ImmunizationDefinition();

        /// <summary>
        /// Creates a list of ImmunizationRecommendation objects from a PHSA models.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <returns>A list of ImmunizationRecommendation objects.</returns>
        public static IList<ImmunizationRecommendation> FromPHSAModelList(IEnumerable<ImmunizationRecommendationResponse> models)
        {
            List<ImmunizationRecommendation> recommendations = new List<ImmunizationRecommendation>();
            ImmunizationRecommendationResponse? recomendationSet = models.FirstOrDefault();
            if (recomendationSet != null)
            {
                string setId = recomendationSet.RecommendationId;
                foreach (RecommendationResponse model in recomendationSet.Recommendations)
                {
                    recommendations.Add(ImmunizationRecommendation.FromPHSAModel(setId, model));
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="recomendationSetId">The recomendation set id of the source system.</param>
        /// <param name="model">The recomendation object to convert.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        private static ImmunizationRecommendation FromPHSAModel(string recomendationSetId, RecommendationResponse model)
        {
            DateCriterion? disseaseEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.text == "Forecast by Disease Eligible Date");
            DateCriterion? diseaseDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.text == "Forecast by Disease Due Date");
            DateCriterion? agentEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.text == "Forecast by Agent Eligible Date");
            DateCriterion? agentDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.text == "Forecast by Agent Eligible Date");

            return new ImmunizationRecommendation()
            {
                RecommendationSetId = recomendationSetId,
                DisseaseEligibleDate = disseaseEligible != null ? DateTime.Parse(disseaseEligible.Value) : null,
                DiseaseDueDate = diseaseDue != null ? DateTime.Parse(diseaseDue.Value) : null,
                AgentEligibleDate = agentEligible != null ? DateTime.Parse(agentEligible.Value) : null,
                AgentDueDate = agentDue != null ? DateTime.Parse(agentDue.Value) : null,
                Status = (ForecastStatus)Enum.Parse(typeof(ForecastStatus), model.ForecastStatus.ForecastStatusText, true),
                TargetDiseases = TargetDisease.FromPHSAModelList(model.TargetDisease),
                Immunization = ImmunizationDefinition.FromPHSAModel(model.VaccineCode),
            };
        }
    }
}