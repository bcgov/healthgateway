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
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.PatientDataAccess;

    /// <summary>
    /// The AutoMapper profile for the Cancer Screening Profile.
    /// </summary>
    public class CancerScreeningProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CancerScreeningProfile"/> class.
        /// </summary>
        public CancerScreeningProfile()
        {
            this.CreateMap<CancerScreeningExam, Services.CancerScreeningExam>()
                .ForMember(
                    d => d.EventTimestampUtc,
                    opts => opts.MapFrom(s => DateFormatter.SpecifyUtc(s.EventTimestampUtc)))
                .ForMember(
                    d => d.ResultTimestamp,
                    opts => opts.MapFrom(s => DateFormatter.SpecifyUtc(s.ResultTimestamp)));
        }
    }
}
