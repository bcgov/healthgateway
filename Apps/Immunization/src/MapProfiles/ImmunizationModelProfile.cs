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
    using System.Collections.Generic;
    using AutoMapper;
    using HealthGateway.Common.Models.Immunization;
    using PatientDataImmunizationRecord = HealthGateway.PatientDataAccess.Immunization;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PatientDataAccess and V2 models.
    /// </summary>
    public class ImmunizationModelProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationModelProfile"/> class.
        /// </summary>
        public ImmunizationModelProfile()
        {
            // V2 field names differ from the ClientApp model shape, so explicit mappings are required.
            this.CreateMap<PatientDataImmunizationRecord, ImmunizationEvent>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.ImmunizationId))
                .ForMember(dest => dest.DateOfImmunization, opts => opts.MapFrom(src => src.OccurrenceDateTime))
                .ForMember(
                    dest => dest.Immunization,
                    opts => opts.MapFrom(
                        (src, _, _, context) => new ImmunizationDefinition
                        {
                            Name = src.VaccineName ?? string.Empty,
                            ImmunizationAgents = context.Mapper.Map<IEnumerable<ImmunizationAgent>>(src.Agents ?? []),
                        }))
                .ForMember(dest => dest.Forecast, opts => opts.MapFrom(src => src.Forecast));
        }
    }
}
