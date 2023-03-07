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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle patients data
    /// </summary>
    public interface IPatientDataRepository
    {
        /// <summary>
        /// Query patient data
        /// </summary>
        /// <param name="query">The query</param>
        /// <returns>The query result</returns>
        Task<QueryResult> Query(PatientDataQuery query);
    }

    /// <summary>
    /// Abstract query record
    /// </summary>
    public abstract record PatientDataQuery;

    /// <summary>
    /// Query parameters for health services
    /// </summary>
    public record HealthServicesQuery(Guid Pid, IEnumerable<HealthServiceCategory> Categories) : PatientDataQuery;

    /// <summary>
    /// Health service categories
    /// </summary>
    public enum HealthServiceCategory
    {
        /// <summary>
        /// BC Transplant Organ Donor
        /// </summary>
        OrganDonor
    }

    /// <summary>
    /// The query result payload
    /// </summary>
    public record QueryResult(IEnumerable<HealthData> Items);

    /// <summary>
    /// abstract record for health data
    /// </summary>
    public abstract record HealthData;

    /// <summary>
    /// abstract class for health service data
    /// </summary>
    public abstract record HealthServiceData : HealthData;

    /// <summary>
    /// BC Transplant organ donor service data
    /// </summary>
    public record OrganDonorRegistration : HealthServiceData
    {
        /// <summary>
        /// The donor registration status
        /// </summary>
        public DonorRegistrationStatus Status { get; set; }

        /// <summary>
        /// Status related message
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// RegistrationFileId
        /// </summary>
        public string? RegistrationFileId { get; set; }
    }

    /// <summary>
    /// Donor registration status
    /// </summary>
    public enum DonorRegistrationStatus
    {
        /// <summary>
        /// Registered patient
        /// </summary>
        Registered,

        /// <summary>
        /// Not registered patient
        /// </summary>
        NonRegistered,

        /// <summary>
        /// Error in registration
        /// </summary>
        Error,

        /// <summary>
        /// Registration is pending
        /// </summary>
        Pending
    }
}
