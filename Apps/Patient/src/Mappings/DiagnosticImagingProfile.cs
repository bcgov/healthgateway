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
    // Disables documentation for internal class.
#pragma warning disable SA1600
    using AutoMapper;
    using HealthGateway.Patient.Models;
    using HealthGateway.PatientDataAccess;

    internal class DiagnosticImagingProfile : Profile
    {
        public DiagnosticImagingProfile()
        {
            this.CreateMap<DiagnosticImagingSummary, DiagnosticImagingData>();
            this.CreateMap<DiagnosticImagingExam, DiagnosticImagingExamData>();
        }
    }
#pragma warning restore SA1600
}
