//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
#pragma warning disable CS1591

namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the Resource Delegate model.
    /// </summary>
    public interface IResourceDelegateDelegate
    {
        /// <summary>
        /// Creates a Resource Delegate record in the database.
        /// </summary>
        /// <param name="resourceDelegate">The resource delegate to create.</param>
        /// <param name="commit">Indicates if the transaction should be persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return record and status.</returns>
        DbResult<ResourceDelegate> Insert(ResourceDelegate resourceDelegate, bool commit);

        /// <summary>
        /// Gets the list of Resource Delegate records for a specific delegate Id from the database.
        /// </summary>
        /// <param name="delegateId">The resource delegate to create.</param>
        /// <param name="page">The page to start at.</param>
        /// <param name="pageSize">The amount of rows to fetch per call.</param>
        /// <returns>A list of resourceDelegates wrapped in a DBResult.</returns>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Team decision")]
        DbResult<IEnumerable<ResourceDelegate>> Get(string delegateId, int page = 0, int pageSize = 500);

        /// <summary>
        /// Gets the list of Resource Delegate records for a date range from the database.
        /// </summary>
        /// <param name="fromDate">The from date.</param>
        /// <param name="toDate">The to date.</param>
        /// <param name="page">The page to start at.</param>
        /// <param name="pageSize">The amount of rows to fetch per call.</param>
        /// <returns>A list of resourceDelegates wrapped in a DBResult.</returns>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Team decision")]
        DbResult<IEnumerable<ResourceDelegate>> Get(DateTime fromDate, DateTime? toDate, int page, int pageSize);

        /// <summary>
        /// Retrieves a count of all dependents.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The number of dependents.</returns>
        Task<int> GetDependentCountAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves daily counts of dependent registrations over a date range.
        /// </summary>
        /// <param name="startDateTimeOffset">The start datetime offset to query.</param>
        /// <param name="endDateTimeOffset">The end datetime offset to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The daily counts of dependent registrations by date.</returns>
        Task<IDictionary<DateOnly, int>> GetDailyDependentRegistrationCountsAsync(DateTimeOffset startDateTimeOffset, DateTimeOffset endDateTimeOffset, CancellationToken ct = default);

        /// <summary>
        /// Gets the total number of delegates associated with each specified dependent from the database.
        /// </summary>
        /// <param name="dependentHdids">The HDIDs corresponding to the dependents whose total delegate counts should be retrieved.</param>
        /// <returns>A dictionary that associates dependent HDIDs with their total number of delegates, wrapped in a DBResult.</returns>
        Task<DbResult<Dictionary<string, int>>> GetTotalDelegateCountsAsync(IEnumerable<string> dependentHdids);

        /// <summary>
        /// Deletes a Resource Delegate record in the database.
        /// </summary>
        /// <param name="resourceDelegate">The model to be deleted.</param>
        /// <param name="commit">Indicates if the transaction should be persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return record and status.</returns>
        DbResult<ResourceDelegate> Delete(ResourceDelegate resourceDelegate, bool commit);

        /// <summary>
        /// Finds a Resource Delegate record in the database.
        /// </summary>
        /// <param name="ownerId">The owner hdid.</param>
        /// <param name="delegateId">The delegated resource hdid.</param>
        /// <returns>A DB result which encapsulates the return record and status.</returns>
        bool Exists(string ownerId, string delegateId);

        /// <summary>
        /// Search resource delegates by criteria specified in the query.
        /// </summary>
        /// <param name="query">The query criteria.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<ResourceDelegateQueryResult> SearchAsync(ResourceDelegateQuery query);
    }

    public record ResourceDelegateQuery
    {
        /// <summary>
        /// Gets the resource owner HDID to search by.
        /// </summary>
        public string? ByOwnerHdid { get; init; }

        /// <summary>
        /// Gets the delegate HDID to search by.
        /// </summary>
        public string? ByDelegateHdid { get; init; }

        /// <summary>
        /// Gets a value indicating whether the associated UserProfile data should be included in the result.
        /// </summary>
        public bool IncludeProfile { get; init; }

        /// <summary>
        /// Gets a value indicating whether the associated dependent data should be included in the result if available.
        /// </summary>
        public bool IncludeDependent { get; init; }

        /// <summary>
        /// Gets the maximum number of records to return. If null, all matching records will be returned.
        /// </summary>
        public int? TakeAmount { get; init; }
    }

    public record ResourceDelegateQueryResult
    {
        /// <summary>
        /// Gets the found items.
        /// </summary>
        public IList<ResourceDelegateQueryResultItem> Items { get; init; } = Array.Empty<ResourceDelegateQueryResultItem>();
    }

    public record ResourceDelegateQueryResultItem
    {
        /// <summary>
        /// Gets the matching resource delegate.
        /// </summary>
        public required ResourceDelegate ResourceDelegate { get; init; }

        /// <summary>
        /// Gets the dependent associated with the resource delegate.
        /// </summary>
        public Dependent? Dependent { get; init; }
    }
}
