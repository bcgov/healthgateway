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
namespace HealthGateway.PatientDataAccess
{
#pragma warning disable SA1600 // Disables documentation for internal class.
    using System;
    using AutoMapper;
    using HealthGateway.PatientDataAccess.Api;

    internal class Mappings : Profile
    {
        public Mappings()
        {
            this.CreateMap<HealthOptionsData, HealthData>()
                .IncludeAllDerived();

            this.CreateMap<HealthDataEntry, HealthData>()
                .IncludeAllDerived()
                .ForMember(hd => hd.Id, opts => opts.MapFrom(hde => hde.HealthDataId))
                .ForMember(exam => exam.FileId, opts => opts.MapFrom(diExam => diExam.HealthDataFileId));

            this.CreateMap<Api.OrganDonorRegistration, OrganDonorRegistration>()
                .ForMember(d => d.Status, opts => opts.MapFrom(s => s.DonorStatus))
                .ForMember(d => d.RegistrationFileId, opts => opts.MapFrom(s => s.HealthOptionsFileId));

            this.CreateMap<Api.DiagnosticImagingExam, DiagnosticImagingExam>();

            this.CreateMap<Api.BcCancerScreening, BcCancerScreening>()
                .ForMember(d => d.EventDateTime, opts => opts.MapFrom(cse => cse.EventTimestampUtc))
                .ForMember(d => d.ResultDateTime, opts => opts.MapFrom(cse => cse.ResultTimestamp.UtcDateTime));

            this.CreateMap<Api.HospitalVisit, HospitalVisit>();

            this.CreateMap<Api.Clinician, Clinician>();

            this.CreateMap<Api.Immunization, Immunization>();

            this.CreateMap<Api.ImmunizationAgent, ImmunizationAgent>();

            this.CreateMap<Api.ImmunizationForecast, ImmunizationForecast>()
                .ForMember(d => d.EligibleDate, opts => opts.MapFrom(s => s.EligibleDate.HasValue ? DateOnly.FromDateTime(s.EligibleDate.Value) : (DateOnly?)null))
                .ForMember(d => d.DueDate, opts => opts.MapFrom(s => DateOnly.FromDateTime(s.DueDate)))
                .ForMember(d => d.ForecastCreateDate, opts => opts.MapFrom(s => s.ForecastCreateDate.HasValue ? DateOnly.FromDateTime(s.ForecastCreateDate.Value) : (DateOnly?)null));

            this.CreateMap<Api.ImmunizationRecommendation, ImmunizationRecommendation>()
                .ForMember(d => d.ForecastCreationDate, opts => opts.MapFrom(s => s.ForecastCreationDate.HasValue ? DateOnly.FromDateTime(s.ForecastCreationDate.Value) : (DateOnly?)null));
            this.CreateMap<Api.RecommendationForecast, RecommendationForecast>();
            this.CreateMap<Api.RecommendationVaccineCode, RecommendationVaccineCode>();
            this.CreateMap<Api.RecommendationTargetDisease, RecommendationTargetDisease>();
            this.CreateMap<Api.RecommendationForecastStatus, RecommendationForecastStatus>();
            this.CreateMap<Api.RecommendationDateCriterion, RecommendationDateCriterion>();
            this.CreateMap<Api.RecommendationDateCriterionCode, RecommendationDateCriterionCode>();
            this.CreateMap<Api.RecommendationSystemCode, RecommendationSystemCode>();
        }
    }
}
#pragma warning restore SA1600
