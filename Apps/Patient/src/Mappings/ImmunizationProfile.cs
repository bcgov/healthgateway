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
namespace HealthGateway.Patient.Mappings
{
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.PatientDataAccess;
    using PatientDataImmunizationAgent = HealthGateway.PatientDataAccess.ImmunizationAgent;
    using PatientDataImmunizationForecast = HealthGateway.PatientDataAccess.ImmunizationForecast;
    using PatientDataImmunizationRecord = HealthGateway.PatientDataAccess.Immunization;

    /// <summary>
    /// The AutoMapper profile for Immunization Records.
    /// </summary>
    public class ImmunizationProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationProfile"/> class.
        /// </summary>
        public ImmunizationProfile()
        {
            this.CreateMap<PatientDataImmunizationRecord, Services.Immunization>()
                .ForMember(
                    d => d.OccurrenceDateTime,
                    opts => opts.MapFrom(s => DateFormatter.SpecifyUtc(s.OccurrenceDateTime)));

            this.CreateMap<PatientDataImmunizationAgent, Services.ImmunizationAgent>();

            this.CreateMap<PatientDataImmunizationForecast, Services.ImmunizationForecast>()
                .ForMember(
                    dest => dest.VaccineCode,
                    opts => opts.MapFrom(src => src.VaccineCode ?? string.Empty));
        }
    }
}
