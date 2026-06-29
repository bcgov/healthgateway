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
namespace HealthGateway.Patient.Mappings
{
    using System;
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.PatientDataAccess;

    /// <summary>
    /// The AutoMapper profile for Hospital Visits.
    public class HospitalVisitProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HospitalVisitProfile"/> class.
        /// </summary>
        public HospitalVisitProfile()
        {
            this.CreateMap<HospitalVisit, Services.HospitalVisit>()
                .ForMember(
                    d => d.AdmitDateTime,
                    opts => opts.MapFrom(s => s.AdmitDateTime.HasValue ? DateFormatter.SpecifyUtc(s.AdmitDateTime.Value) : (DateTime?)null));
            this.CreateMap<HospitalVisit, Services.HospitalVisit>()
                .ForMember(
                    d => d.EndDateTime,
                    opts => opts.MapFrom(s => s.EndDateTime.HasValue ? DateFormatter.SpecifyUtc(s.EndDateTime.Value) : (DateTime?)null));
        }
    }
}
