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
#pragma warning disable SA1201 // Elements should appear in the correct order
    using System;

    /// <summary>
    /// abstract record for health data.
    /// </summary>
    public abstract record HealthData
    {
        /// <summary>
        /// Gets or sets the health data's id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the health data's report file id.
        /// </summary>
        public string? FileId { get; set; }
    }

    /// <summary>
    /// The details of a diagnostic imaging exam.
    /// </summary>
    public record DiagnosticImagingExam : HealthData
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
        /// Gets or sets the exam's date.
        /// </summary>
        public DateTime? ExamDate { get; set; }
    }

    /// <summary>
    /// Diagnostic image exam statuses.
    /// </summary>
    public enum DiagnosticImagingStatus
    {
        /// <summary>
        /// Exam is scheduled.
        /// </summary>
        Scheduled,

        /// <summary>
        /// Exam is in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Exam is finalized.
        /// </summary>
        Finalized,

        /// <summary>
        /// Exam result is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// Exam is completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Exam is amended.
        /// </summary>
        Amended,
    }
}
#pragma warning restore SA1201
