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
namespace HealthGateway.ClinicalDocument.MapProfiles
{
    using System;
    using AutoMapper;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.ClinicalDocument.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA and UI Models.
    /// </summary>
    public class ClinicalDocumentRecordProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClinicalDocumentRecordProfile"/> class.
        /// </summary>
        public ClinicalDocumentRecordProfile()
        {
            // ServiceDates specified with an empty time and +00:00 offset need ToUniversalTime() to return the correct day
            this.CreateMap<PhsaClinicalDocumentRecord, ClinicalDocumentRecord>()
                .ForMember(dest => dest.ServiceDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ServiceDate.ToUniversalTime())));
        }
    }
}
