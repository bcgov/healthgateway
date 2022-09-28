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
    using AutoMapper;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA Immunization and Common Models.
    /// </summary>
    public class AgentProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentProfile"/> class.
        /// </summary>
        public AgentProfile()
        {
            this.CreateMap<ImmunizationAgentResponse, ImmunizationAgent>();
            this.CreateMap<SystemCode, ImmunizationAgent>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Display));
        }
    }
}
