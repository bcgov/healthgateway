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
    using HealthGateway.Patient.Models;
    using HealthGateway.Patient.Services;
    using HealthGateway.PatientDataAccess;

    /// <summary>
    /// The AutoMapper profile for the Diagnostic Imaging Exams.
    /// </summary>
    public class DiagnosticImagingExamProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticImagingExamProfile"/> class.
        /// </summary>
        public DiagnosticImagingExamProfile()
        {
            this.CreateMap<DiagnosticImagingExam, DiagnosticImagingExamData>()
                .ForMember(
                    d => d.ExamStatus,
                    opts => opts.MapFrom(s => MapDiagnosticImagingExamStatus(s.ExamStatus)));
        }

        private static DiagnosticImagingStatus MapDiagnosticImagingExamStatus(DiagnosticImagingExamStatus status)
        {
            return status switch
            {
                DiagnosticImagingExamStatus.Scheduled => DiagnosticImagingStatus.Scheduled,
                DiagnosticImagingExamStatus.InProgress => DiagnosticImagingStatus.InProgress,
                DiagnosticImagingExamStatus.Finalized => DiagnosticImagingStatus.Finalized,
                DiagnosticImagingExamStatus.Pending => DiagnosticImagingStatus.Pending,
                DiagnosticImagingExamStatus.Completed => DiagnosticImagingStatus.Completed,
                DiagnosticImagingExamStatus.Amended => DiagnosticImagingStatus.Amended,
                _ => DiagnosticImagingStatus.Unknown,
            };
        }
    }
}
