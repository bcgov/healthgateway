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
        /// Specifies that the EMPI data source is to be queried against using cache if available.
        /// </summary>
        EmpiCache,

        /// <summary>
        /// Specifies that the PHSA data source is to be queried against.
        /// </summary>
        Phsa,

        /// <summary>
        /// Specifies that the PHSA data source is to be queried against using cache if available.
        /// </summary>
        PhsaCache,

        /// <summary>
        /// Specifies that all data sources are to be queried against.
        /// </summary>
        All,

        /// <summary>
        /// Specifies that all data sources are to be queried against using cache if available.
        /// </summary>
        AllCache,
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
        /// <returns>The patient model wrapped in an api result object.</returns>
        Task<PatientQueryResult> Query(PatientQuery query, CancellationToken ct);
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
    /// <param name="Source">The patient detail source to query.</param>
    public record PatientDetailsQuery(string? Phn = null, string? Hdid = null, PatientDetailSource Source = PatientDetailSource.All) : PatientQuery;
}
