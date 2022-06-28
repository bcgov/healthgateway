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
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.AspNetCore.SignalR;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA Immunization and Common Models.
    /// </summary>
    public class EventProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventProfile"/> class.
        /// </summary>
        public EventProfile()
        {
            this.CreateMap<ImmunizationViewResponse, ImmunizationEvent>()
                .ForMember(dest => dest.DateOfImmunization, opt => opt.MapFrom(src => src.OccurrenceDateTime))
                .ForMember(dest => dest.Immunization, opt =>
                    opt.MapFrom((src, dest, _, context) => new ImmunizationDefinition()
                    {
                        Name = src.Name,
                        ImmunizationAgents = context.Mapper.Map<IEnumerable<ImmunizationAgent>>(src.ImmunizationAgents),
                    }))
                .ForMember(dest => dest.Forecast, opt => opt.MapFrom(src => src.ImmunizationForecast));
        }
    }
}
