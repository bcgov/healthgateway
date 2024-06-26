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
namespace HealthGateway.PatientDataAccess.Api
{
#pragma warning disable SA1600 // Disables documentation for internal class.
#pragma warning disable SA1602 // Disables documentation for internal class.
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(HealthDataJsonConverter))]
    [ExcludeFromCodeCoverage]
    internal abstract record HealthDataEntry
    {
        public string? HealthDataId { get; set; }

        public string? HealthDataFileId { get; set; }
    }

    // Class is currently not being passed by the front end
    [ExcludeFromCodeCoverage]
    internal record ClinicalDocument : HealthDataEntry
    {
        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? FacilityName { get; set; }

        public string? Discipline { get; set; }

        public DateTime? ServiceDate { get; set; }

        public string? SourceSystemId { get; set; }
    }

    // Class is currently not being passed by the front end
    [ExcludeFromCodeCoverage]
    internal record LaboratoryOrder : HealthDataEntry
    {
        public string? SourceSystemId { get; set; }

        public string? Phn { get; set; }

        public string? OrderingProviderIds { get; set; }

        public string? OrderingProvider { get; set; }

        public string? ReportingLab { get; set; }

        public string? Location { get; set; }

        public string? OrmOrOru { get; set; }

        public DateTime? MessageDateTime { get; set; }

        public string? MessageId { get; set; }

        public string? AdditionalData { get; set; }

        public bool? ReportAvailable { get; set; }

        public IEnumerable<LaboratoryResult>? LabResults { get; set; }
    }

    // Class is currently not being passed by the front end
    [ExcludeFromCodeCoverage]
    internal record LaboratoryResult
    {
        public string? Id { get; set; }

        public string? TestType { get; set; }

        public bool? OutOfRange { get; set; }

        public DateTime? CollectedDateTime { get; set; }

        public string? TestStatus { get; set; }

        public string? ResultDescription { get; set; }

        public string? LabResultOutcome { get; set; }

        public string? ReceivedDateTime { get; set; }

        public string? ResultDateTime { get; set; }

        public string? Loinc { get; set; }

        public string? LoincName { get; set; }

        public string? ResultTitle { get; set; }

        public string? ResultLink { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal record DiagnosticImagingExam : HealthDataEntry
    {
        public string? ProcedureDescription { get; set; }

        public string? BodyPart { get; set; }

        public string? Modality { get; set; }

        public string? Organization { get; set; }

        public string? HealthAuthority { get; set; }

        public DiagnosticImagingStatus Status { get; set; }

        public DateTime? ExamDate { get; set; }

        public bool? IsUpdated { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal record BcCancerScreening : HealthDataEntry
    {
        public CancerScreeningType EventType { get; set; }

        public string? ProgramName { get; set; }

        public DateTime EventTimestampUtc { get; set; }

        public DateTimeOffset ResultTimestamp { get; set; }
    }
}
#pragma warning restore SA1600
#pragma warning restore SA1602
