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
namespace HealthGateway.Laboratory.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile that defines mappings between PHSA and Health Gateway Models.
    /// </summary>
    public class LaboratoryOrderProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryOrderProfile"/> class.
        /// </summary>
        public LaboratoryOrderProfile()
        {
            this.CreateMap<PhsaLaboratoryOrder, LaboratoryOrder>()
                .ForMember(dest => dest.TestStatus, opt => opt.MapFrom(src => src.PlisTestStatus))
                .ForMember(dest => dest.ReportAvailable, opt => opt.MapFrom(src => src.PdfReportAvailable))
                .ForMember(dest => dest.LaboratoryTests, opt => opt.MapFrom(src => src.LabBatteries))
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.OrderStatus = src.PlisTestStatus switch
                        {
                            "Held" or "Partial" or "Pending" => "Pending",
                            _ => src.PlisTestStatus,
                        };
                    });
        }
    }
}
