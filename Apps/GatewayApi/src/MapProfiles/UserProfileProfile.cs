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
    using System;
    using AutoMapper;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between DB Model UserProfile and API Model UserProfileModel.
    /// </summary>
    public class UserProfileProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileProfile"/> class.
        /// </summary>
        public UserProfileProfile()
        {
            this.CreateMap<UserProfile, UserProfileModel>()
                .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Email)))
                .ForMember(dest => dest.IsSMSNumberVerified, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.SMSNumber)))
                .ForMember(dest => dest.AcceptedTermsOfService, opt => opt.MapFrom(src => src.TermsOfServiceId != Guid.Empty))
                .ReverseMap();
        }
    }
}
