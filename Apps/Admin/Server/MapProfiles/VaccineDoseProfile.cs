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
    using System;
    using AutoMapper;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models.Immunization;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA and UI Models.
    /// </summary>
    public class VaccineDoseProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineDoseProfile"/> class.
        /// </summary>
        public VaccineDoseProfile()
        {
            this.CreateMap<VaccineDoseResponse, VaccineDose>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date == null ? null : (DateOnly?)DateOnly.FromDateTime(src.Date.Value)));
        }
    }
}
