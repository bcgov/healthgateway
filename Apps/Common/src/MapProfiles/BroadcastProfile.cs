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
namespace HealthGateway.Common.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA and front-end models.
    /// </summary>
    public class BroadcastProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastProfile"/> class.
        /// </summary>
        public BroadcastProfile()
        {
            this.CreateMap<BroadcastResponse, Broadcast>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName ?? string.Empty))
                .ForMember(dest => dest.DisplayText, opt => opt.MapFrom(src => src.DisplayText ?? string.Empty));
            this.CreateMap<Broadcast, BroadcastRequest>();
        }
    }
}
