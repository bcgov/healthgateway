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
    using System;
    using AutoMapper;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// An AutoMapper profile that defines mappings between PHSA and Health Gateway Models.
    /// </summary>
    public class VaccineStatusProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusProfile"/> class.
        /// </summary>
        public VaccineStatusProfile()
        {
            this.CreateMap<VaccineStatusResult, VaccineStatus>()
                .ForMember(dest => dest.Doses, opt => opt.MapFrom(src => src.DoseCount))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => Enum.Parse<VaccineState>(src.StatusIndicator)));
        }
    }
}
