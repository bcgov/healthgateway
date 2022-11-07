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
    using AutoMapper;
    using HealthGateway.Medication.Models.ODR;

    /// <summary>
    /// An AutoMapper profile that defines mappings between PHSA and Health Gateway Models.
    /// </summary>
    public class PharmacyProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PharmacyProfile"/> class.
        /// </summary>
        public PharmacyProfile()
        {
            this.CreateMap<Pharmacy, Models.Pharmacy>()
                .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address.Line1))
                .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.Address.Line2))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Address.Country))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Address.Province));
        }
    }
}
