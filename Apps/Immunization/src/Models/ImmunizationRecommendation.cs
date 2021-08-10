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
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Parser;

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
            DateCriterion? disseaseEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Disease Eligible Date");
            DateCriterion? diseaseDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Disease Due Date");
            DateCriterion? agentEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Agent Eligible Date");
            DateCriterion? agentDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Agent Due Date");

            return new ImmunizationRecommendation(TargetDisease.FromPHSAModelList(model.TargetDisease))
            {
                RecommendationSetId = recomendationSetId,
                DisseaseEligibleDate = disseaseEligible != null ? DateTime.Parse(disseaseEligible.Value, CultureInfo.CurrentCulture) : null,
                DiseaseDueDate = diseaseDue != null ? DateTime.Parse(diseaseDue.Value, CultureInfo.CurrentCulture) : null,
                AgentEligibleDate = agentEligible != null ? DateTime.Parse(agentEligible.Value, CultureInfo.CurrentCulture) : null,
                AgentDueDate = agentDue != null ? DateTime.Parse(agentDue.Value, CultureInfo.CurrentCulture) : null,
                Status = model.ForecastStatus.ForecastStatusText,
                Immunization = DefinitionParser.FromPHSAModel(model.VaccineCode),
            };
        }
    }
}