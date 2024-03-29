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
namespace HealthGateway.Medication.MapProfiles
{
    using System;
    using AutoMapper;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.Salesforce;

    /// <summary>
    /// An AutoMapper profile that defines mappings between Salesforce and Health Gateway Models.
    /// </summary>
    public class SpecialAuthorityRequestProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialAuthorityRequestProfile"/> class.
        /// </summary>
        public SpecialAuthorityRequestProfile()
        {
            this.CreateMap<SpecialAuthorityRequest, MedicationRequest>()
                .ForMember(dest => dest.RequestedDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.RequestedDate)))
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate == null ? (DateOnly?)null : DateOnly.FromDateTime(src.EffectiveDate.Value)))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate == null ? (DateOnly?)null : DateOnly.FromDateTime(src.ExpiryDate.Value)));
        }
    }
}
