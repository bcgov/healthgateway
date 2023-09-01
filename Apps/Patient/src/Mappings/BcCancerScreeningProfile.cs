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
    /// The AutoMapper profile for BcCancerScreening.
    /// </summary>
    public class BcCancerScreeningProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BcCancerScreeningProfile"/> class.
        /// </summary>
        public BcCancerScreeningProfile()
        {
            this.CreateMap<BcCancerScreening, Services.BcCancerScreening>()
                .ForMember(
                    d => d.EventDateTime,
                    opts => opts.MapFrom(s => DateFormatter.SpecifyUtc(s.EventDateTime)))
                .ForMember(
                    d => d.ResultDateTime,
                    opts => opts.MapFrom(s => DateFormatter.SpecifyUtc(s.ResultDateTime)));
        }
    }
}
