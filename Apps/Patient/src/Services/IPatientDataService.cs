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
namespace HealthGateway.Patient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Utils;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Models;

    /// <summary>
    /// Provides access to patient related data services.
    /// </summary>
    public interface IPatientDataService
    {
        /// <summary>
        /// Query data services.
        /// </summary>
        /// <param name="query">The query message.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The Response message.</returns>
        Task<PatientDataResponse> QueryAsync(PatientDataQuery query, CancellationToken ct = default);

        /// <summary>
        /// Query patient files.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>Patient file or null if not found.</returns>
        Task<PatientFileResponse?> QueryAsync(PatientFileQuery query, CancellationToken ct = default);
    }

    /// <summary>
    /// Query message for patient data services.
    /// </summary>
    /// <param name="Hdid">The patient hdid.</param>
    /// <param name="PatientDataTypes">The data types to query.</param>
    [ExcludeFromCodeCoverage]
    public record PatientDataQuery(string Hdid, IEnumerable<PatientDataType> PatientDataTypes);

    /// <summary>
    /// Response message with patient data.
    /// </summary>
    /// <param name="Items">list of patient data information.</param>
    [ExcludeFromCodeCoverage]
    public record PatientDataResponse(IEnumerable<PatientData> Items);

    /// <summary>
    /// abstract record that contains patient data.
    /// </summary>
    [JsonConverter(typeof(PatientDataJsonConverter))]
    [KnownType(typeof(OrganDonorRegistration))]
    [KnownType(typeof(DiagnosticImagingExam))]
    [KnownType(typeof(BcCancerScreening))]
    [KnownType(typeof(HospitalVisit))]
    [KnownType(typeof(ImmunizationRecord))]
    [ExcludeFromCodeCoverage]
    public abstract record PatientData
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets the type of the patient data.
        /// </summary>
        public abstract string Type { get; }
    }

    /// <summary>
    /// Organ donor patient data.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record OrganDonorRegistration : PatientData
    {
        /// <summary>
        /// Gets the registration status.
        /// </summary>
        public required OrganDonorRegistrationStatus Status { get; init; } = OrganDonorRegistrationStatus.NotRegistered;

        /// <summary>
        /// Gets the message associated with the donor registration status.
        /// </summary>
        public required string? StatusMessage { get; init; }

        /// <summary>
        /// Gets the file ID associated with the donor registration.
        /// </summary>
        public required string? RegistrationFileId { get; init; }

        /// <summary>
        /// Gets the organ donor registration link text derived from the status.
        /// </summary>
        public required string OrganDonorRegistrationLinkText { get; init; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(OrganDonorRegistration);
    }

    /// <summary>
    /// Diagnostic imaging exam patient data.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record DiagnosticImagingExam : PatientData
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
        public DiagnosticImagingStatus ExamStatus { get; set; }

        /// <summary>
        /// Gets or sets the exam's file id.
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// Gets or sets the exam's date.
        /// </summary>
        public DateOnly? ExamDate { get; set; }

        /// <summary>
        /// Gets or sets if an exam has been updated.
        /// </summary>
        public bool? IsUpdated { get; set; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(DiagnosticImagingExam);
    }

    /// <summary>
    /// BC Cancer screening exam patient data.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record BcCancerScreening : PatientData
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
        /// Gets or sets the file id.
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// Gets or sets the event datetime.
        /// </summary>
        public DateTime EventDateTime { get; set; }

        /// <summary>
        /// Gets or sets the result datetime.
        /// </summary>
        public DateTime ResultDateTime { get; set; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(BcCancerScreening);
    }

    /// <summary>
    /// The details of a hospital visit.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record HospitalVisit : PatientData
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
        /// Gets the provider.
        /// </summary>
        public string? Provider { get; init; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(HospitalVisit);
    }

    /// <summary>
    /// The details of an immunization record.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ImmunizationRecord : PatientData
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
        public DateTime? OccurrenceDateTime { get; init; }

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

        /// <inheritdoc/>
        public override string Type { get; } = "Immunization";
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

        /// <summary>
        /// Gets the system.
        /// </summary>
        public string? System { get; init; }
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

    /// <summary>
    /// Query patient files.
    /// </summary>
    /// <param name="Hdid">Patient's hdid.</param>
    /// <param name="FileId">File id.</param>
    [ExcludeFromCodeCoverage]
    public record PatientFileQuery(string Hdid, string FileId);

    /// <summary>
    /// Patient file response.
    /// </summary>
    /// <param name="Content">The file content.</param>
    /// <param name="ContentType">The file content type.</param>
    [ExcludeFromCodeCoverage]
    public record PatientFileResponse(IEnumerable<byte> Content, string ContentType);

    // Disable documentation for internal class.
#pragma warning disable SA1600
    internal class PatientDataJsonConverter : PolymorphicJsonConverter<PatientData>
    {
        protected override string ResolveDiscriminatorValue(PatientData value)
        {
            return value.Type;
        }

        protected override Type? ResolveType(string? discriminatorValue)
        {
            return discriminatorValue switch
            {
                nameof(OrganDonorRegistration) => typeof(OrganDonorRegistration),
                nameof(DiagnosticImagingExam) => typeof(DiagnosticImagingExam),
                nameof(BcCancerScreening) => typeof(BcCancerScreening),
                nameof(HospitalVisit) => typeof(HospitalVisit),
                "Immunization" => typeof(ImmunizationRecord),
                _ => null,
            };
        }
    }
#pragma warning restore SA1600
}
