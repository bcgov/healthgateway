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
namespace HealthGateway.GatewayApi.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Common.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between Patient and DependentInformation.
    /// </summary>
    public class DependentInformationProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentInformationProfile"/> class.
        /// </summary>
        public DependentInformationProfile()
        {
            this.CreateMap<PatientModel, DependentInformation>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Birthdate))
                .ForMember(dest => dest.PHN, opt => opt.MapFrom(src => src.PersonalHealthNumber))
                .ReverseMap();
        }
    }
}
