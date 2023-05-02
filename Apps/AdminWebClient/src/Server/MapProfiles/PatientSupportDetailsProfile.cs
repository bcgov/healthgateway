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
namespace HealthGateway.Admin.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Admin.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// An AutoMapper profile class which defines mappings between DB models and data transfer objects.
    /// </summary>
    public class PatientSupportDetailsProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientSupportDetailsProfile"/> class.
        /// </summary>
        public PatientSupportDetailsProfile()
        {
            this.CreateMap<Common.Data.Models.UserProfile, PatientSupportDetails>()
                .ForMember(dest => dest.Hdid, opt => opt.MapFrom(src => src.HdId))
                .ReverseMap();

            this.CreateMap<ResourceDelegate, PatientSupportDetails>()
                .ForMember(d => d.Hdid, opts => opts.MapFrom(s => s.ProfileHdid))
                .ForMember(d => d.PersonalHealthNumber, opts => opts.Ignore())
                .ForMember(d => d.LastLoginDateTime, opts => opts.Ignore())
                .ForMember(d => d.CreatedDateTime, opts => opts.Ignore());
        }
    }
}
