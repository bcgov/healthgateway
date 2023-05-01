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
    using System.Linq;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// An AutoMapper profile class which defines a mapping between a data access model and data transfer object.
    /// </summary>
    public class PatientSupportDetailsProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientSupportDetailsProfile"/> class.
        /// </summary>
        public PatientSupportDetailsProfile()
        {
            this.CreateMap<PatientModel?, PatientSupportDetails>()
                .ForMember(dest => dest.WarningMessage, opt => opt.MapFrom(src => src == null ? null : GetWarningFromResponseCode(src.ResponseCode)))
                .ForMember(dest => dest.WarningMessage, opt => opt.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Hdid, opt => opt.NullSubstitute(string.Empty))
                .ForMember(dest => dest.PersonalHealthNumber, opt => opt.MapFrom(src => src == null ? string.Empty : src.Phn))
                .ForMember(dest => dest.PersonalHealthNumber, opt => opt.NullSubstitute(string.Empty))
                .ForMember(dest => dest.Birthdate, opt => opt.MapFrom(src => src == null ? null : (DateOnly?)DateOnly.FromDateTime(src.Birthdate)))
                .ForMember(dest => dest.ProfileCreatedDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileLastLoginDateTime, opt => opt.Ignore());
        }

        private static string? GetWarningFromResponseCode(string? responseCode)
        {
            return responseCode?.Length > 0 ? responseCode.Split('|', StringSplitOptions.TrimEntries).ElementAtOrDefault(1) : null;
        }
    }
}
