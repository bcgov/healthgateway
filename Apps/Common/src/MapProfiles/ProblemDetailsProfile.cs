﻿// -------------------------------------------------------------------------
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
    using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between ProblemDetailsException and .Net MVC's ProblemDetails class.
    /// </summary>
    public class ProblemDetailsProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsProfile"/> class.
        /// </summary>
        public ProblemDetailsProfile()
        {
            this.CreateMap<HealthGateway.Common.Data.ErrorHandling.ProblemDetails, ProblemDetails>()
                .ForMember(
                    dest => dest.Status,
                    opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(
                    dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type));
        }
    }
}
