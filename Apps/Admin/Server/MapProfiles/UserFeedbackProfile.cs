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
namespace HealthGateway.Admin.Server.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between DB and UI Models.
    /// </summary>
    public class UserFeedbackProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserFeedbackProfile"/> class.
        /// </summary>
        public UserFeedbackProfile()
        {
            this.CreateMap<UserFeedback, UserFeedbackView>()
                .ForMember(dest => dest.ClientType, opt => opt.MapFrom(src => src.ClientCode))
                .ReverseMap();

            this.CreateMap<UserFeedbackTag, UserFeedbackTagView>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserFeedbackTagId))
                .ForMember(dest => dest.TagId, opt => opt.MapFrom(src => src.AdminTagId))
                .ForMember(dest => dest.FeedbackId, opt => opt.MapFrom(src => src.UserFeedbackId))
                .ReverseMap();
        }
    }
}
