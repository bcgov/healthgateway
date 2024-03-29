﻿// -------------------------------------------------------------------------
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
    using HealthGateway.PatientDataAccess;
    using DiagnosticImagingStatus = HealthGateway.Patient.Models.DiagnosticImagingStatus;

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
            this.CreateMap<DiagnosticImagingExam, Services.DiagnosticImagingExam>()
                .ForMember(
                    d => d.ExamStatus,
                    opts => opts.MapFrom(s => MapDiagnosticImagingExamStatus(s.Status)))
                .ForMember(
                    d => d.ExamDate,
                    opts => opts.MapFrom(s => s.ExamDate == null ? (DateOnly?)null : DateOnly.FromDateTime(s.ExamDate.Value)));
        }

        private static DiagnosticImagingStatus MapDiagnosticImagingExamStatus(PatientDataAccess.DiagnosticImagingStatus status)
        {
            return status switch
            {
                PatientDataAccess.DiagnosticImagingStatus.Scheduled => DiagnosticImagingStatus.Pending,
                PatientDataAccess.DiagnosticImagingStatus.InProgress => DiagnosticImagingStatus.Pending,
                PatientDataAccess.DiagnosticImagingStatus.Finalized => DiagnosticImagingStatus.Final,
                PatientDataAccess.DiagnosticImagingStatus.Pending => DiagnosticImagingStatus.Pending,
                PatientDataAccess.DiagnosticImagingStatus.Completed => DiagnosticImagingStatus.Pending,
                PatientDataAccess.DiagnosticImagingStatus.Amended => DiagnosticImagingStatus.Amended,
                _ => DiagnosticImagingStatus.Unknown,
            };
        }
    }
}
