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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Common.Utils;

namespace HealthGateway.Patient.Services
{
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
    public record PatientDataQuery(string Hdid, PatientDataType[] PatientDataTypes);

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
        /// The type of the patient data
        /// </summary>
        [JsonIgnore]
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
            Status = status;
            StatusMessage = statusMessage;
            RegistrationFileId = registrationFileId;
        }

        /// <summary>
        /// The registration status
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// Message related to the status
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Registration file id
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
    public record PatientFileResponse(byte[] Content, string ContentType);

    internal class PatientDataJsonConverter : PolymorphicJsonConverter<PatientData>
    {
        protected override string ResolveDiscriminatorValue(PatientData value) => value.Type;

        protected override Type? ResolveType(string discriminatorValue) =>
            discriminatorValue switch
            {
                nameof(OrganDonorRegistrationData) => typeof(OrganDonorRegistrationData),

                _ => null
            };
    }
}
