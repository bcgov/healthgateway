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
#pragma warning disable CA1716 // Identifiers should not match keywords
        DbResult<IEnumerable<ResourceDelegate>> Get(string delegateId, int page, int pageSize);
#pragma warning restore CA1716 // Identifiers should not match keywords

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
        /// Gets the count of dependents from the database.
        /// </summary>
        /// <param name="offset">The clients offset to get to UTC.</param>
        /// <returns>Total number of dependents.</returns>
        IDictionary<DateTime, int> GetDailyDependentCount(TimeSpan offset);

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
        /// <param name="query">the query criteria.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public Task<ResourceDelegateQueryResult> Search(ResourceDelegateQuery query);
    }

    public record ResourceDelegateQuery
    {
        /// <summary>
        /// Gets or sets search by owner's hdid.
        /// </summary>
        public string? ByOwnerHdid { get; set; }

        /// <summary>
        /// Gets or sets search by delegate hdid.
        /// </summary>
        public string? ByDelegateHdid { get; set; }
    }

    public record ResourceDelegateQueryResult
    {
        /// <summary>
        /// gets or sets the found items.
        /// </summary>
        public IEnumerable<ResourceDelegate> Items { get; set; } = Array.Empty<ResourceDelegate>();
    }
}
