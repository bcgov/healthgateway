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

namespace HealthGateway.Immunization.MapUtils
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class ImmunizationRecommendationMapUtils
    {
        /// <summary>
        /// Creates a list of ImmunizationRecommendation objects from a PHSA models.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A list of ImmunizationRecommendation objects.</returns>
        public static IList<ImmunizationRecommendation> FromPhsaModelList(IEnumerable<ImmunizationRecommendationResponse> models, IMapper autoMapper)
        {
            List<ImmunizationRecommendation> recommendations = new();
            models.ToList()
                .ForEach(
                    model =>
                    {
                        string? vaccineGroup = model.Recommendations.FirstOrDefault(r => r.TargetDisease == null)?.VaccineCode.VaccineCodes.FirstOrDefault()?.Display;
                        List<string> vaccinations = model.Recommendations.Where(r => r.TargetDisease != null).Select(v => v.VaccineCode.VaccineCodeText).ToList();
                        string recommendedVaccinations = $"{vaccineGroup} ({string.Join(", ", vaccinations)})";

                        recommendations.AddRange(
                            model.Recommendations.Select(
                                recommendation => FromPhsaModel(model.RecommendationId, recommendation, autoMapper, recommendedVaccinations)));
                    });

            return recommendations;
        }

        /// <summary>
        /// Creates a ImmunizationEvent object from a PHSA model.
        /// </summary>
        /// <param name="recommendationSetId">The recommendation set id of the source system.</param>
        /// <param name="model">The recommendation object to convert.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <param name="recommendedVaccinations">The recommended vaccinations.</param>
        /// <returns>The newly created ImmunizationEvent object.</returns>
        private static ImmunizationRecommendation FromPhsaModel(string recommendationSetId, RecommendationResponse model, IMapper autoMapper, string recommendedVaccinations)
        {
            DateCriterion? diseaseEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Disease Eligible Date");
            DateCriterion? diseaseDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Disease Due Date");
            DateCriterion? agentEligible = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Agent Eligible Date");
            DateCriterion? agentDue = model.DateCriterions.FirstOrDefault(x => x.DateCriterionCode.Text == "Forecast by Agent Due Date");

            IList<TargetDisease> targetDiseases = model.TargetDisease != null ? autoMapper.Map<IList<TargetDisease>>(model.TargetDisease.TargetDiseaseCodes) : new List<TargetDisease>();
            return new ImmunizationRecommendation(targetDiseases)
            {
                RecommendedVaccinations = model.TargetDisease == null ? recommendedVaccinations : string.Empty,
                RecommendationSetId = recommendationSetId,
                DiseaseEligibleDate = diseaseEligible != null ? DateTime.Parse(diseaseEligible.Value, CultureInfo.CurrentCulture) : null,
                DiseaseDueDate = diseaseDue != null ? DateTime.Parse(diseaseDue.Value, CultureInfo.CurrentCulture) : null,
                AgentEligibleDate = agentEligible != null ? DateTime.Parse(agentEligible.Value, CultureInfo.CurrentCulture) : null,
                AgentDueDate = agentDue != null ? DateTime.Parse(agentDue.Value, CultureInfo.CurrentCulture) : null,
                Status = model.ForecastStatus.ForecastStatusText,
                Immunization = autoMapper.Map<ImmunizationDefinition>(model.VaccineCode),
            };
        }
    }
}
