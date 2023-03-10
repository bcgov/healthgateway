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

    /// <summary>
    /// Provides access to patient related data services
    /// </summary>
    public interface IPatientDataService
    {
        /// <summary>
        /// Query data services
        /// </summary>
        /// <param name="query">The query message</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The Response message</returns>
        Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct);

        /// <summary>
        /// Query patient files
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>Patient file or null if not found</returns>
        Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct);
    }

    /// <summary>
    /// Query message for patient data services
    /// </summary>
    /// <param name="Hdid">The patient hdid</param>
    /// <param name="PatientDataTypes">The data types to query</param>
    public record PatientDataQuery(string Hdid, IEnumerable<PatientDataType> PatientDataTypes);

    /// <summary>
    /// Patiend data types
    /// </summary>
    public enum PatientDataType
    {
        /// <summary>
        /// Organ donor registration status
        /// </summary>
        OrganDonorRegistrationStatus,
    }

    /// <summary>
    /// Response message with patient data
    /// </summary>
    /// <param name="Items">list of patient data information</param>
    public record PatientDataResponse(IEnumerable<PatientData> Items);

    /// <summary>
    /// abstract record that contains patient data
    /// </summary>
    [JsonConverter(typeof(PatientDataJsonConverter))]
    [KnownType(typeof(OrganDonorRegistrationData))]
    public abstract record PatientData
    {
        /// <summary>
        /// Gets or sets the type of the patient data
        /// </summary>
        public abstract string Type { get; set; }
    }

    /// <summary>
    /// Organ donor patient data
    /// </summary>
    public record OrganDonorRegistrationData : PatientData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganDonorRegistrationData"/> class.
        /// </summary>
        /// <param name="status">The registration status</param>
        /// <param name="statusMessage">Optional message related to the status</param>
        /// <param name="registrationFileId">Optional registration file id</param>
        public OrganDonorRegistrationData(string status, string? statusMessage, string? registrationFileId)
        {
            this.Status = status;
            this.StatusMessage = statusMessage;
            this.RegistrationFileId = registrationFileId;
        }

        /// <summary>
        /// Gets or sets the donor registration status
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Gets or sets the message associated with the donor registration status
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the file ID associated with the donor registration
        /// </summary>
        public string? RegistrationFileId { get; set; }

        /// <inheritdoc/>
        public override string Type { get; set; } = nameof(OrganDonorRegistrationData);
    }

    /// <summary>
    /// Query patient files
    /// </summary>
    /// <param name="Hdid">Patient's hdid</param>
    /// <param name="FileId">File id</param>
    public record PatientFileQuery(string Hdid, string FileId);

    /// <summary>
    /// Patient file response
    /// </summary>
    /// <param name="Content">The file content</param>
    /// <param name="ContentType">The file content type</param>
    public record PatientFileResponse(IEnumerable<byte> Content, string ContentType);

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Team decision")]
    internal class PatientDataJsonConverter : PolymorphicJsonConverter<PatientData>
    {
        /// <inheritdoc/>
        protected override string ResolveDiscriminatorValue(PatientData value) => value.Type;

        /// <inheritdoc/>
        protected override Type? ResolveType(string discriminatorValue) =>
            discriminatorValue switch
            {
                nameof(OrganDonorRegistrationData) => typeof(OrganDonorRegistrationData),

                _ => null,
            };
    }
}
