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
namespace HealthGateway.AccountDataAccess.Patient
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Represents the patient detail source to determine what to query.
    /// </summary>
    public enum PatientDetailSource
    {
        /// <summary>
        /// Specifies that the EMPI data source is to be queried against.
        /// </summary>
        Empi,

        /// <summary>
        /// Specifies that the PHSA data source is to be queried against.
        /// </summary>
        Phsa,

        /// <summary>
        /// Specifies that both EMPI and PHSA (if necessary) data sources are to be queried against.
        /// </summary>
        All,
    }

    /// <summary>
    /// Handle Patient data.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The patient model wrapped in a patient query result object.</returns>
        Task<PatientQueryResult> QueryAsync(PatientQuery query, CancellationToken ct = default);

        /// <summary>
        /// Returns true if data source can be accessed and false if it cannot.
        /// </summary>
        /// <param name="hdid">The hdid to query on.</param>
        /// <param name="dataSource">The data source to check.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The blocked access or null if not found.</returns>
        Task<bool> CanAccessDataSourceAsync(string hdid, DataSource dataSource, CancellationToken ct = default);

        /// <summary>
        /// Gets the blocked access record.
        /// </summary>
        /// <param name="hdid">The hdid to query on.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The blocked access or null if not found.</returns>
        Task<BlockedAccess?> GetBlockedAccessRecordsAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Gets the blocked access's data sources for the hdid.
        /// </summary>
        /// <param name="hdid">The hdid to query on.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A list of blocked access data source values.</returns>
        Task<IEnumerable<DataSource>> GetDataSourcesAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Block access to data sources associated with the hdid.
        /// </summary>
        /// <param name="command">The command details used to block access.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The agent audit entry created from the operation.</returns>
        Task BlockAccessAsync(BlockAccessCommand command, CancellationToken ct = default);
    }

    /// <summary>
    /// The search query.
    /// </summary>
    public abstract record PatientQuery;

    /// <summary>
    /// The query result.
    /// </summary>
    /// <param name="Items">The result.</param>
    public record PatientQueryResult(IEnumerable<PatientModel> Items);

    /// <summary>
    /// The patient details query.
    /// </summary>
    /// <param name="Phn">The phn to search.</param>
    /// <param name="Hdid">The Hdid to search.</param>
    /// <param name="Source">The patient detail source to search.</param>
    /// <param name="UseCache">The value that indicates whether cache should be used when querying for the result.</param>
    public record PatientDetailsQuery(
        string? Phn = null,
        string? Hdid = null,
        PatientDetailSource Source = PatientDetailSource.All,
        bool UseCache = true) : PatientQuery;

    /// <summary>
    /// The blocked access command to add, update or delete.
    /// </summary>
    /// <param name="Hdid">The hdid associated with the blocked access record.</param>
    /// <param name="DataSources">The data sources to block access.</param>
    /// <param name="Reason">The reason to block access.</param>
    public record BlockAccessCommand(
        string Hdid,
        IEnumerable<DataSource> DataSources,
        string Reason);
}
