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
    using System.Collections.Generic;
    using HealthGateway.Patient.Constants;
    using HealthGateway.PatientDataAccess;

    /// <summary>
    /// Query message for patient data services for health options.
    /// </summary>
    /// <param name="Hdid">The patient hdid.</param>
    /// <param name="HealthOptionsTypes">The data types to query.</param>
    public record PatientOptionsQuery(string Hdid, IEnumerable<HealthOptionsType> HealthOptionsTypes);

    /// <summary>
    /// Response message with patient data health option records.
    /// </summary>
    /// <param name="Items">list of patient data information.</param>
    public record PatientOptionsResponse(IEnumerable<BasePatientOptionRecord> Items);

    /// <summary>
    /// Organ donor health option patient data.
    /// </summary>
    public record OrganDonorRegistrationInfo : BasePatientOptionRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganDonorRegistrationInfo"/> class.
        /// </summary>
        /// <param name="status">The registration status.</param>
        /// <param name="statusMessage">Optional message related to the status.</param>
        /// <param name="registrationFileId">Optional registration file id.</param>
        public OrganDonorRegistrationInfo(DonorRegistrationStatus status, string? statusMessage, string? registrationFileId)
        {
            this.Status = status;
            this.StatusMessage = statusMessage;
            this.RegistrationFileId = registrationFileId;
        }

        /// <summary>
        /// Gets or sets the registration status.
        /// </summary>
        public DonorRegistrationStatus Status { get; set; } = DonorRegistrationStatus.NotRegistered;

        /// <summary>
        /// Gets or sets the message associated with the donor registration status.
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the file ID associated with the donor registration.
        /// </summary>
        public string? RegistrationFileId { get; set; }

        /// <inheritdoc/>
        public override string Type => nameof(OrganDonorRegistrationInfo);
    }
}
