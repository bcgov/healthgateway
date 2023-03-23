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
namespace HealthGateway.PatientDataAccess
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract class for health service data.
    /// </summary>
    public abstract record BaseHealthData : BasePatientData;

    /// <summary>
    /// The collection of diagnostic imaging exams document.
    /// </summary>
    public record DiagnosticImagingSummary : BaseHealthData
    {
        /// <summary>
        /// Gets or sets the collection of diagnostic imaging exams.
        /// </summary>
        public IEnumerable<DiagnosticImagingExam>? Exams { get; set; }
    }

    /// <summary>
    /// The details of a diagnostic imaging exam.
    /// </summary>
    public record DiagnosticImagingExam
    {
        /// <summary>
        /// Gets or sets the exam's procedure description.
        /// </summary>
        public string? ProcedureDescription { get; set; }

        /// <summary>
        /// Gets or sets the exam's body part.
        /// </summary>
        public string? BodyPart { get; set; }

        /// <summary>
        /// Gets or sets the exam's modality.
        /// </summary>
        public string? Modality { get; set; }

        /// <summary>
        /// Gets or sets the exam's organization.
        /// </summary>
        public string? Organization { get; set; }

        /// <summary>
        /// Gets or sets the exam's health authority.
        /// </summary>
        public string? HealthAuthority { get; set; }

        /// <summary>
        /// Gets or sets the exam's status.
        /// </summary>
        public DiagnosticImagingStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the exam's file id.
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// Gets or sets the exam's date.
        /// </summary>
        public DateTime? ExamDate { get; set; }
    }
}
