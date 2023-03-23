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
namespace HealthGateway.PatientDataAccess.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Utils;

#pragma warning disable SA1600
#pragma warning disable SA1602

    internal enum HealthDataCategory
    {
        Laboratory,
        COVID19Laboratory,
        ClinicalDocument,
        DiagnosticImaging,
    }

    internal enum DiStatus
    {
        Scheduled,
        InProgress,
        Finalized,
        Pending,
        Completed,
        Amended,
    }

    internal record HealthDataResult(HealthDataMetadata Metadata, IEnumerable<HealthData> Data);

    internal record HealthDataMetadata;

    [JsonConverter(typeof(HealthDataJsonConverter))]
    internal abstract record HealthData
    {
        public string? HealthDataId { get; set; }

        public string? HealthDataType { get; set; }

        public string? HealthDataFileId { get; set; }
    }

    internal record DiSummary : HealthData
    {
        public IEnumerable<DiExam>? Exams { get; set; }
    }

    internal record ClinicalDocument : HealthData
    {
        public string? Name { get; set; }

        public string? Type { get; set; }

        public string? FacilityName { get; set; }

        public string? Discipline { get; set; }

        public DateTime? ServiceDate { get; set; }

        public string? SourceSystemId { get; set; }
    }

    internal record LaboratoryOrder : HealthData
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

    internal record DiExam
    {
        public string? ProcedureDescription { get; set; }

        public string? BodyPart { get; set; }

        public string? Modality { get; set; }

        public string? Organization { get; set; }

        public string? HealthAuthority { get; set; }

        public DiStatus Status { get; set; }

        public string? FileId { get; set; }

        public DateTime? ExamDate { get; set; }
    }

    internal class HealthDataJsonConverter : PolymorphicJsonConverter<HealthData>
    {
        protected override string Discriminator => "healthDataType";

        protected override Type? ResolveType(string discriminatorValue)
        {
            return discriminatorValue switch
            {
                "Laboratory" => typeof(LaboratoryOrder),
                "Immunization" => typeof(HealthData),
                "COVID19Laboratory" => typeof(LaboratoryOrder),
                "Medication" => typeof(HealthData),
                "ClinicalDocument" => typeof(ClinicalDocument),
                "HealthVisit" => typeof(HealthData),
                "NyNotes" => typeof(HealthData),
                "SpecialAuthority" => typeof(HealthData),
                "DiagnosticImaging" => typeof(DiSummary),
                _ => null,
            };
        }
    }

#pragma warning restore SA1600
#pragma warning restore SA1602
}
