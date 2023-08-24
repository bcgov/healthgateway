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
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The Response message.</returns>
        Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct);

        /// <summary>
        /// Query patient files.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>Patient file or null if not found.</returns>
        Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct);
    }

    /// <summary>
    /// Query message for patient data services.
    /// </summary>
    /// <param name="Hdid">The patient hdid.</param>
    /// <param name="PatientDataTypes">The data types to query.</param>
    public record PatientDataQuery(string Hdid, IEnumerable<PatientDataType> PatientDataTypes);

    /// <summary>
    /// Response message with patient data.
    /// </summary>
    /// <param name="Items">list of patient data information.</param>
    public record PatientDataResponse(IEnumerable<PatientData> Items);

    /// <summary>
    /// abstract record that contains patient data.
    /// </summary>
    [JsonConverter(typeof(PatientDataJsonConverter))]
    [KnownType(typeof(OrganDonorRegistration))]
    [KnownType(typeof(DiagnosticImagingExam))]
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
        public DateTime? ExamDate { get; set; }

        /// <summary>
        /// Gets or sets if an exam has been updated.
        /// </summary>
        public bool? IsUpdated { get; set; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(DiagnosticImagingExam);
    }

    /// <summary>
    /// Cancer screening exam patient data.
    /// </summary>
    public record CancerScreeningExam : PatientData
    {
        /// <summary>
        /// Gets or sets the cancer screening's event type.
        /// </summary>
        public CancerScreeningType CancerScreeningType { get; set; }

        /// <summary>
        /// Gets or sets the cancer screening's program name.
        /// </summary>
        public string? ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the cancer screening's date.
        /// </summary>
        public DateTime EventTimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the cancer screening's result timestamp.
        /// </summary>
        public DateTime ResultTimestamp { get; set; }

        /// <inheritdoc/>
        public override string Type { get; } = nameof(CancerScreeningExam);
    }

    /// <summary>
    /// Query patient files.
    /// </summary>
    /// <param name="Hdid">Patient's hdid.</param>
    /// <param name="FileId">File id.</param>
    public record PatientFileQuery(string Hdid, string FileId);

    /// <summary>
    /// Patient file response.
    /// </summary>
    /// <param name="Content">The file content.</param>
    /// <param name="ContentType">The file content type.</param>
    public record PatientFileResponse(IEnumerable<byte> Content, string ContentType);

// Disable documentation for internal class.
#pragma warning disable SA1600
    internal class PatientDataJsonConverter : PolymorphicJsonConverter<PatientData>
    {
        protected override string ResolveDiscriminatorValue(PatientData value)
        {
            return value.Type;
        }

        protected override Type? ResolveType(string discriminatorValue)
        {
            return discriminatorValue switch
            {
                nameof(OrganDonorRegistration) => typeof(OrganDonorRegistration),
                nameof(DiagnosticImagingExam) => typeof(DiagnosticImagingExam),
                _ => null,
            };
        }
    }
#pragma warning restore SA1600
}
