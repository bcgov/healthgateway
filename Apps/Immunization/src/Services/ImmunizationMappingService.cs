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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AutoMapper;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class ImmunizationMappingService(IMapper mapper, IConfiguration configuration) : IImmunizationMappingService
    {
        private TimeZoneInfo LocalTimeZone => DateFormatter.GetLocalTimeZone(configuration);

        /// <inheritdoc/>
        public ImmunizationEvent MapToImmunizationEvent(ImmunizationViewResponse source)
        {
            ImmunizationEvent dest = mapper.Map<ImmunizationViewResponse, ImmunizationEvent>(source);

            dest.DateOfImmunization = DateFormatter.SpecifyTimeZone(dest.DateOfImmunization, this.LocalTimeZone);

            return dest;
        }

        /// <inheritdoc/>
        public IList<ImmunizationRecommendation> MapToImmunizationRecommendations(IEnumerable<ImmunizationRecommendationResponse> source)
        {
            return source.SelectMany(s => s.Recommendations.Select(r => MapToImmunizationRecommendation(s, r))).ToList();

            ImmunizationRecommendation MapToImmunizationRecommendation(ImmunizationRecommendationResponse s, RecommendationResponse r)
            {
                return new ImmunizationRecommendation
                {
                    RecommendedVaccinations = r.TargetDisease == null ? GetRecommendedVaccinationsString(s.Recommendations.ToList()) : string.Empty,
                    RecommendationSetId = s.RecommendationId,
                    DiseaseEligibleDate = GetDateFromCriterions(r.DateCriterions, "Forecast by Disease Eligible Date"),
                    DiseaseDueDate = GetDateFromCriterions(r.DateCriterions, "Forecast by Disease Due Date"),
                    AgentEligibleDate = GetDateFromCriterions(r.DateCriterions, "Forecast by Agent Eligible Date"),
                    AgentDueDate = GetDateFromCriterions(r.DateCriterions, "Forecast by Agent Due Date"),
                    Status = r.ForecastStatus.ForecastStatusText,
                    Immunization = mapper.Map<VaccineCode, ImmunizationDefinition>(r.VaccineCode),
                    TargetDiseases = r.TargetDisease == null ? [] : mapper.Map<IEnumerable<SystemCode>, IEnumerable<TargetDisease>>(r.TargetDisease.TargetDiseaseCodes),
                };
            }

            static string GetRecommendedVaccinationsString(IList<RecommendationResponse> recommendations)
            {
                string? vaccineGroup = recommendations.FirstOrDefault(r => r.TargetDisease == null)?.VaccineCode.VaccineCodes.FirstOrDefault()?.Display;
                IEnumerable<string> vaccinations = recommendations.Where(r => r.TargetDisease != null).Select(v => v.VaccineCode.VaccineCodeText);
                return $"{vaccineGroup} ({string.Join(", ", vaccinations)})";
            }

            static DateOnly? GetDateFromCriterions(IEnumerable<DateCriterion> criterions, string code)
            {
                DateCriterion? criterion = criterions.FirstOrDefault(x => x.DateCriterionCode.Text == code);
                return criterion?.Value != null ? DateOnly.Parse(criterion.Value, CultureInfo.CurrentCulture) : null;
            }
        }

        /// <inheritdoc/>
        public LoadStateModel MapToLoadStateModel(PhsaLoadState source)
        {
            return mapper.Map<PhsaLoadState, LoadStateModel>(source);
        }

        /// <inheritdoc/>
        public VaccineStatus MapToVaccineStatus(VaccineStatusResult source, string? personalHealthNumber)
        {
            VaccineStatus dest = mapper.Map<VaccineStatusResult, VaccineStatus>(source);

            dest.PersonalHealthNumber = personalHealthNumber;
            if (source.FederalVaccineProof?.Data != null)
            {
                dest.FederalVaccineProof = new EncodedMedia
                {
                    Type = "application/pdf",
                    Encoding = "base64",
                    Data = GetGenericVaccineProof(),
                };
            }

            return dest;
        }

        private static string GetGenericVaccineProof()
        {
            const string resourceName = "HealthGateway.Immunization.Assets.VaccineProof.pdf";

            Assembly? assembly = Assembly.GetAssembly(typeof(ImmunizationMappingService));
            Stream resourceStream = assembly!.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("Proof of vaccination not found.");
            using MemoryStream memoryStream = new();
            resourceStream.CopyTo(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }
}
