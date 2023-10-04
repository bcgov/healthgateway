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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientDataAccess;
    using BcCancerScreening = HealthGateway.PatientDataAccess.BcCancerScreening;
    using DiagnosticImagingExam = HealthGateway.PatientDataAccess.DiagnosticImagingExam;
    using OrganDonorRegistration = HealthGateway.PatientDataAccess.OrganDonorRegistration;

    /// <summary>
    /// Patient data access mappings.
    /// </summary>
    public class PatientDataAccessMappings : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDataAccessMappings"/> class.
        /// </summary>
        public PatientDataAccessMappings()
        {
            this.CreateMap<PatientDataType, HealthCategory>()
                .ConvertUsing(
                    (source, _, _) =>
                    {
                        return source switch
                        {
                            PatientDataType.OrganDonorRegistrationStatus => HealthCategory.OrganDonorRegistrationStatus,
                            PatientDataType.DiagnosticImaging => HealthCategory.DiagnosticImaging,
                            PatientDataType.BcCancerScreening => HealthCategory.BcCancerScreening,
                            _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                        };
                    });
            this.CreateMap<HealthData, PatientData>()
                .ConvertUsing<PatientDataConverter>();
            this.CreateMap<PatientDataType, DataSource>()
                .ConvertUsing(
                    (source, _, _) =>
                    {
                        return source switch
                        {
                            PatientDataType.OrganDonorRegistrationStatus => DataSource.OrganDonorRegistration,
                            PatientDataType.DiagnosticImaging => DataSource.DiagnosticImaging,
                            PatientDataType.BcCancerScreening => DataSource.BcCancerScreening,
                            _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                        };
                    });
        }

#pragma warning disable SA1600
#pragma warning disable SA1602
        internal class PatientDataConverter : ITypeConverter<HealthData, PatientData>
        {
            public PatientData Convert(HealthData source, PatientData destination, ResolutionContext context)
            {
                return source switch
                {
                    OrganDonorRegistration hd => context.Mapper.Map<Services.OrganDonorRegistration>(hd),
                    DiagnosticImagingExam hd => context.Mapper.Map<Services.DiagnosticImagingExam>(hd),
                    BcCancerScreening hd => context.Mapper.Map<Services.BcCancerScreening>(hd),
                    _ => throw new NotImplementedException($"{source.GetType().Name} is not mapped to {nameof(PatientData)}"),
                };
            }
        }
    }
#pragma warning restore SA1600
#pragma warning restore SA1602
}
