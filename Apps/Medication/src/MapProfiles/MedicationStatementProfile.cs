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
    using HealthGateway.Medication.Models.ODR;

    /// <summary>
    /// An AutoMapper profile that defines mappings between ODR and Health Gateway Models.
    /// </summary>
    public class MedicationStatementProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationStatementProfile"/> class.
        /// </summary>
        public MedicationStatementProfile()
        {
            this.CreateMap<MedicationResult, MedicationStatement>()
                .ForMember(dest => dest.DispensedDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.DispenseDate)))
                .ForMember(dest => dest.PractitionerSurname, opt => opt.MapFrom(src => src.Practitioner != null ? src.Practitioner.Surname : string.Empty))
                .ForMember(dest => dest.PrescriptionIdentifier, opt => opt.MapFrom(src => src.PrescriptionNumber))
                .ForMember(
                    dest => dest.MedicationSummary,
                    opt => opt.MapFrom(
                        src =>
                            new MedicationSummary
                            {
                                Din = src.Din,
                                Quantity = src.Quantity,
                                GenericName = src.GenericName,
                                BrandName = "Unknown brand name",
                            }));
        }
    }
}
