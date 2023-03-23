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
    /// <summary>
    /// abstract class for health service data.
    /// </summary>
    public abstract record BaseHealthOptions : BasePatientData;

    /// <summary>
    /// BC Transplant organ donor service data.
    /// </summary>
    public record OrganDonorRegistration : BaseHealthOptions
    {
        /// <summary>
        /// Gets or sets the donor registration status.
        /// </summary>
        public DonorRegistrationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the donor registration status.
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the file ID associated with the donor registration.
        /// </summary>
        public string? RegistrationFileId { get; set; }
    }
}
