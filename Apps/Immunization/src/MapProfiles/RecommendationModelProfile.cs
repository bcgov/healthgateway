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
namespace HealthGateway.Immunization.MapProfiles
{
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.PatientDataAccess;
    using PatientDataImmunizationRecommendation = HealthGateway.PatientDataAccess.ImmunizationRecommendation;
    using PatientDataRecommendationDateCriterion = HealthGateway.PatientDataAccess.RecommendationDateCriterion;
    using PatientDataRecommendationDateCriterionCode = HealthGateway.PatientDataAccess.RecommendationDateCriterionCode;
    using PatientDataRecommendationForecast = HealthGateway.PatientDataAccess.RecommendationForecast;
    using PatientDataRecommendationForecastStatus = HealthGateway.PatientDataAccess.RecommendationForecastStatus;
    using PatientDataRecommendationTargetDisease = HealthGateway.PatientDataAccess.RecommendationTargetDisease;
    using PatientDataRecommendationVaccineCode = HealthGateway.PatientDataAccess.RecommendationVaccineCode;

    /// <summary>
    /// Maps V2 patient-data recommendations to the existing recommendation contract.
    /// </summary>
    public class RecommendationModelProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationModelProfile"/> class.
        /// </summary>
        public RecommendationModelProfile()
        {
            this.CreateMap<PatientDataImmunizationRecommendation, ImmunizationRecommendationResponse>()
                .ForMember(dest => dest.ForecastCreationDate, opts => opts.MapFrom(src => src.ForecastCreationDate ?? default))
                .ForMember(dest => dest.RecommendationId, opts => opts.MapFrom(src => src.RecommendationId ?? string.Empty))
                .ForMember(dest => dest.RecommendationSourceSystem, opts => opts.MapFrom(src => src.RecommendationSourceSystem ?? string.Empty))
                .ForMember(dest => dest.RecommendationSourceSystemId, opts => opts.MapFrom(src => src.RecommendationSourceSystemId ?? string.Empty));
            this.CreateMap<PatientDataRecommendationForecast, RecommendationResponse>()
                .ForMember(dest => dest.DateCriterions, opts => opts.MapFrom(src => src.DateCriterion ?? Enumerable.Empty<PatientDataRecommendationDateCriterion>()))
                .ForMember(dest => dest.ForecastStatus, opts => opts.MapFrom(src => src.ForecastStatus ?? new PatientDataRecommendationForecastStatus()))
                .ForMember(dest => dest.VaccineCode, opts => opts.MapFrom(src => src.VaccineCode ?? new PatientDataRecommendationVaccineCode()))
                .ForMember(dest => dest.TargetDisease, opts => opts.PreCondition(src => src.TargetDisease != null));
            this.CreateMap<PatientDataRecommendationVaccineCode, VaccineCode>()
                .ForMember(dest => dest.VaccineCodeText, opts => opts.MapFrom(src => src.VaccineCodeText ?? string.Empty))
                .ForMember(dest => dest.VaccineCodes, opts => opts.MapFrom(src => src.VaccineCodes ?? Enumerable.Empty<ForecastCode>()));
            this.CreateMap<PatientDataRecommendationTargetDisease, TargetDiseaseResponse>()
                .ForMember(dest => dest.TargetDiseaseCodes, opts => opts.MapFrom(src => src.TargetDiseaseCodes ?? Enumerable.Empty<ForecastCode>()));
            this.CreateMap<PatientDataRecommendationForecastStatus, ForecastStatusModel>()
                .ForMember(dest => dest.ForecastStatusText, opts => opts.MapFrom(src => src.ForecastStatusText ?? string.Empty))
                .ForMember(dest => dest.ForcastCodes, opts => opts.MapFrom(src => src.ForecastCodes ?? Enumerable.Empty<ForecastCode>()));
            this.CreateMap<PatientDataRecommendationDateCriterion, DateCriterion>()
                .ForMember(dest => dest.DateCriterionCode, opts => opts.MapFrom(src => src.DateCriterionCode ?? new PatientDataRecommendationDateCriterionCode()))
                .ForMember(dest => dest.Value, opts => opts.MapFrom(src => src.Value ?? string.Empty));
            this.CreateMap<PatientDataRecommendationDateCriterionCode, DateCriterionCode>()
                .ForMember(dest => dest.Text, opts => opts.MapFrom(src => src.Text ?? string.Empty));
            this.CreateMap<ForecastCode, SystemCode>()
                .ForMember(dest => dest.System, opts => opts.MapFrom(src => src.System ?? string.Empty))
                .ForMember(dest => dest.Code, opts => opts.MapFrom(src => src.Code ?? string.Empty))
                .ForMember(dest => dest.Display, opts => opts.MapFrom(src => src.Display ?? string.Empty))
                .ForMember(dest => dest.CommonType, opts => opts.MapFrom(src => src.CommonType ?? string.Empty));
        }
    }
}
