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
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handle patients data.
    /// </summary>
    public interface IPatientDataRepository
    {
        /// <summary>
        /// Query patient data.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The query result.</returns>
        Task<PatientDataQueryResult> Query(PatientDataQuery query, CancellationToken ct);
    }

    /// <summary>
    /// Abstract query record.
    /// </summary>
    public abstract record PatientDataQuery;

    /// <summary>
    /// Query parameters for health services.
    /// </summary>
    public record HealthServicesQuery(Guid Pid, IEnumerable<HealthServiceCategory> Categories) : PatientDataQuery;

    /// <summary>
    /// Query parameters for health data.
    /// </summary>
    public record HealthDataQuery(Guid Pid, IEnumerable<HealthDataCategory> Categories) : PatientDataQuery;

    /// <summary>
    /// Query patient files.
    /// </summary>
    public record PatientFileQuery(Guid Pid, string FileId) : PatientDataQuery;

    /// <summary>
    /// The health data query result payload.
    /// </summary>
    public record PatientDataQueryResult(IEnumerable<HealthData> Items);

    /// <summary>
    /// Represents a patient file.
    /// </summary>
    public record PatientFile(string FileId, IEnumerable<byte> Content, string ContentType) : HealthData;
}
