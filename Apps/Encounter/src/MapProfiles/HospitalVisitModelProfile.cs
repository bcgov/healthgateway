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
namespace HealthGateway.Encounter.MapProfiles
{
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA and UI Models.
    /// </summary>
    public class HospitalVisitModelProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalVisitModelProfile"/> class.
        /// </summary>
        public HospitalVisitModelProfile()
        {
            this.CreateMap<HospitalVisit, HospitalVisitModel>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Clinicians.Where(c => c.RoleDescription == "Attending Physician").Select(c => c.DisplayName)))
                .ReverseMap();
        }
    }
}
