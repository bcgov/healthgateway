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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// abstract record for health data.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
    [ExcludeFromCodeCoverage]
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

        /// <summary>
        /// Gets or sets whether the diagnostic imaging has recently been updated.
        /// </summary>
        public bool? IsUpdated { get; set; }
    }

    /// <summary>
    /// The details of a BC Cancer screening exam.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record BcCancerScreening : HealthData
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public BcCancerScreeningType EventType { get; set; }

        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        public string? ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the event datetime.
        /// </summary>
        public DateTime EventDateTime { get; set; }

        /// <summary>
        /// Gets or sets the result datetime.
        /// </summary>
        public DateTime ResultDateTime { get; set; }
    }

    /// <summary>
    /// The details of a hospital visit.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record HospitalVisit : HealthData
    {
        /// <summary>
        /// Gets the encounter id.
        /// </summary>
        public string? EncounterId { get; init; }

        /// <summary>
        /// Gets the facility.
        /// </summary>
        public string? Facility { get; init; }

        /// <summary>
        /// Gets the health service.
        /// </summary>
        public string? HealthService { get; init; }

        /// <summary>
        /// Gets the visit type.
        /// </summary>
        public string? VisitType { get; init; }

        /// <summary>
        /// Gets the health authority.
        /// </summary>
        public string? HealthAuthority { get; init; }

        /// <summary>
        /// Gets the admit date time.
        /// </summary>
        public DateTime? AdmitDateTime { get; init; }

        /// <summary>
        /// Gets the end date time.
        /// </summary>
        public DateTime? EndDateTime { get; init; }

        /// <summary>
        /// Gets the clinicians associated with the hospital visit.
        /// </summary>
        public IEnumerable<Clinician>? Clinicians { get; init; }
    }

    /// <summary>
    /// Represents a clinician associated with a hospital visit.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record Clinician
    {
        /// <summary>
        /// Gets the clinician's display name.
        /// </summary>
        public string? DisplayName { get; init; }

        /// <summary>
        /// Gets the clinician's role description.
        /// </summary>
        public string? RoleDescription { get; init; }
    }

    /// <summary>
    /// The details of an immunization record.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ImmunizationRecord : HealthData
    {
        /// <summary>
        /// Gets the immunization id.
        /// </summary>
        public string? ImmunizationId { get; init; }

        /// <summary>
        /// Gets the vaccine name.
        /// </summary>
        public string? VaccineName { get; init; }

        /// <summary>
        /// Gets the immunization status.
        /// </summary>
        public string? Status { get; init; }

        /// <summary>
        /// Gets the occurrence date time.
        /// </summary>
        public DateTime OccurrenceDateTime { get; init; }

        /// <summary>
        /// Gets the provider or clinic.
        /// </summary>
        public string? ProviderOrClinic { get; init; }

        /// <summary>
        /// Gets the immunization agents.
        /// </summary>
        public IEnumerable<ImmunizationAgent>? Agents { get; init; }

        /// <summary>
        /// Gets the immunization forecast.
        /// </summary>
        public ImmunizationForecast? Forecast { get; init; }
    }

    /// <summary>
    /// Represents an immunization agent.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ImmunizationAgent
    {
        /// <summary>
        /// Gets the agent code.
        /// </summary>
        public string? Code { get; init; }

        /// <summary>
        /// Gets the agent name.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the lot number.
        /// </summary>
        public string? LotNumber { get; init; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string? ProductName { get; init; }
    }

    /// <summary>
    /// Represents an immunization forecast.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ImmunizationForecast
    {
        /// <summary>
        /// Gets the forecast status.
        /// </summary>
        public string? ForecastStatus { get; init; }

        /// <summary>
        /// Gets the vaccine code.
        /// </summary>
        public string? VaccineCode { get; init; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public string? DisplayName { get; init; }

        /// <summary>
        /// Gets the eligible date.
        /// </summary>
        public DateOnly? EligibleDate { get; init; }

        /// <summary>
        /// Gets the due date.
        /// </summary>
        public DateOnly DueDate { get; init; }

        /// <summary>
        /// Gets the forecast create date.
        /// </summary>
        public DateOnly? ForecastCreateDate { get; init; }
    }

#pragma warning disable CS1591 // Recommendation payload models mirror the producer contract.
#pragma warning disable SA1600 // Recommendation payload models mirror the producer contract.
    [ExcludeFromCodeCoverage]
    public record ImmunizationRecommendation : HealthData
    {
        public string? RecommendationId { get; init; }

        public string? RecommendationSourceSystem { get; init; }

        public string? RecommendationSourceSystemId { get; init; }

        public DateOnly? ForecastCreationDate { get; init; }

        public IEnumerable<RecommendationForecast>? Recommendations { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationForecast
    {
        public RecommendationVaccineCode? VaccineCode { get; init; }

        public RecommendationTargetDisease? TargetDisease { get; init; }

        public RecommendationForecastStatus? ForecastStatus { get; init; }

        public IEnumerable<RecommendationDateCriterion>? DateCriterion { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationVaccineCode
    {
        public string? VaccineCodeText { get; init; }

        public IEnumerable<ForecastCode>? VaccineCodes { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationTargetDisease
    {
        public IEnumerable<ForecastCode>? TargetDiseaseCodes { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationForecastStatus
    {
        public IEnumerable<ForecastCode>? ForecastCodes { get; init; }

        public string? ForecastStatusText { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationDateCriterion
    {
        public RecommendationDateCriterionCode? DateCriterionCode { get; init; }

        public string? Value { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record RecommendationDateCriterionCode
    {
        public string? Text { get; init; }
    }

    [ExcludeFromCodeCoverage]
    public record ForecastCode
    {
        public string? System { get; init; }

        public string? Code { get; init; }

        public string? Display { get; init; }

        public string? CommonType { get; init; }
    }
#pragma warning restore SA1600
#pragma warning restore CS1591

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

    /// <summary>
    /// BC Cancer screening types.
    /// </summary>
    public enum BcCancerScreeningType
    {
        /// <summary>
        /// Cancer screening recall.
        /// </summary>
        Recall,

        /// <summary>
        /// Cancer screening result.
        /// </summary>
        Result,
    }
}
#pragma warning restore SA1201
