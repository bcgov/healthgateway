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
namespace HealthGateway.Patient.Models
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Patient.Constants;
    using HealthGateway.PatientDataAccess;

    /// <summary>
    /// Query message for patient data services for health data.
    /// </summary>
    /// <param name="Hdid">The patient HDID.</param>
    /// <param name="HealthDataTypes">The data types to query.</param>
    public record PatientDataQuery(string Hdid, IEnumerable<HealthDataType> HealthDataTypes);

    /// <summary>
    /// Response message with patient data health data records.
    /// </summary>
    /// <param name="Items">list of patient data information.</param>
    public record PatientDataResponse(IEnumerable<BasePatientDataRecord> Items);

    /// <summary>
    /// Data model for PatientDataAccess's DiagnosticImagingSummary.
    /// </summary>
    public record DiagnosticImagingData
    {
        /// <summary>
        /// Gets or sets the diagnostic imaging summary.
        /// </summary>
        public IEnumerable<DiagnosticImagingExamData> Exams { get; set; } = new List<DiagnosticImagingExamData>();
    }

    /// <summary>
    /// The details of a diagnostic imaging exam.
    /// </summary>
    public record DiagnosticImagingExamData : BasePatientDataRecord
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

        /// <inheritdoc/>
        public override string Type => nameof(DiagnosticImagingExamData);
    }
}
